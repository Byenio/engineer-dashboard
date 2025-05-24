create database telemetry;

use telemetry;

CREATE TABLE Ranks (
    id int PRIMARY KEY auto_increment,
    name VARCHAR(20) not null,
    icon TEXT not null,
    pointsMin int not null,
    pointsMax int not null
);

CREATE TABLE Drivers (
    id BIGINT PRIMARY KEY auto_increment,
    username VARCHAR(48) not null,
    ELO int not null default 1000,
    rankId int not null,
    FOREIGN KEY (rankId) REFERENCES Ranks(id)
);

CREATE TABLE Tracks (
    id int PRIMARY KEY auto_increment,
    name VARCHAR(20) not null
);

CREATE TABLE Races (
    id BIGINT PRIMARY KEY auto_increment,
    date DATE not null,
    trackId int not null,
    aiDifficulty int not null,
    raceLength int not null,
    FOREIGN KEY (trackId) REFERENCES Tracks(id)
);

CREATE TABLE RaceResults (
    id BIGINT PRIMARY KEY auto_increment,
    raceId BIGINT not null,
    driverId BIGINT not null,
    startPos int not null,
    finishPos int not null,
    hasFastestLap int not null,
    penalties int not null,
    dnf int not null,
    sessionTime float not null,
    points int not null,
    carDamage int not null,
    FOREIGN KEY (raceId) REFERENCES Races(id),
    FOREIGN KEY (driverId) REFERENCES Drivers(id)
);

CREATE TABLE Laps (
    id BIGINT PRIMARY KEY auto_increment,
    raceResultId BIGINT not null,
    position int not null,
    tyreCompound int not null ,
    deltaToLeader int not null,
    deltaToCarInFront int not null,
    tyreWear float not null,
    lapTime int not null,
    FOREIGN KEY (raceResultId) REFERENCES RaceResults(id)
);

CREATE TABLE Stints (
    id BIGINT PRIMARY KEY auto_increment,  
    raceResultId BIGINT not null,
    tyreCompound int not null,
    startLap int not null,
    endLap int not null,
    pitStopTime int not null,
    FOREIGN KEY (raceResultId) REFERENCES RaceResults(id)
);


DELIMITER $$

CREATE TRIGGER update_driver_elo
AFTER INSERT ON RaceResults
FOR EACH ROW
BEGIN
    DECLARE difficultyMultiplier INT;
    DECLARE sfDiff INT;
    DECLARE points INT;
    DECLARE dmg INT;
    DECLARE dnf INT;
    DECLARE pen INT;
    
    DECLARE wghtSfDiff FLOAT;
    DECLARE wghtPoints FLOAT;
    DECLARE wghtDmg FLOAT;
    DECLARE wghtDnf FLOAT;
    DECLARE wghtPen FLOAT;
    
    DECLARE currentElo INT;
    DECLARE deltaElo FLOAT;
    DECLARE newElo INT;
    
    -- Pobierz difficultyMultiplier z tabeli Races
    SELECT aiDifficulty + raceLength
    INTO difficultyMultiplier
    FROM Races
    WHERE id = NEW.raceId;

    -- Obliczenia komponentów
    SET sfDiff = NEW.startPos - NEW.finishPos;
    SET points = NEW.points + NEW.hasFastestLap;
    SET dmg = 100 - NEW.carDamage;
    SET dnf = (20 - NEW.startPos) * NEW.dnf;
    SET pen = NEW.penalties;

    SET wghtSfDiff = sfDiff * difficultyMultiplier;
    SET wghtPoints = points * (difficultyMultiplier / 100);
    SET wghtDmg = dmg * (difficultyMultiplier / 100);
    SET wghtDnf = dnf * difficultyMultiplier;
    SET wghtPen = pen;

    -- Pobierz aktualne ELO kierowcy
    SELECT ELO INTO currentElo FROM Drivers WHERE id = NEW.driverId;

    -- Oblicz zmianę ELO
    SET deltaElo = (wghtSfDiff + wghtPoints + wghtDmg - wghtDnf - wghtPen) / (currentElo / 10);
    SET newElo = currentElo + ROUND(deltaElo);

    -- Zaktualizuj ELO w tabeli Drivers
    UPDATE Drivers SET ELO = newElo WHERE id = NEW.driverId;
END$$

DELIMITER ;
