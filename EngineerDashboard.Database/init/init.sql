\c telemetry;

CREATE TABLE Ranks (
    id INT PRIMARY KEY,
    name VARCHAR(20) NOT NULL,
    icon TEXT NOT NULL,
    pointsMin INT NOT NULL,
    pointsMax INT
);

CREATE TABLE Drivers (
    id BIGINT PRIMARY KEY,
    username VARCHAR(48) NOT NULL,
    ELO INT NOT NULL DEFAULT 1000,
    rankId INT NOT NULL REFERENCES Ranks(id)
);

CREATE TABLE Tracks (
    id INT PRIMARY KEY,
    name VARCHAR(20) NOT NULL
);

CREATE TABLE Races (
    id BIGINT PRIMARY KEY,
    date DATE NOT NULL,
    trackId INT NOT NULL REFERENCES Tracks(id),
    aiDifficulty INT NOT NULL,
    raceLength INT NOT NULL
);

CREATE TABLE RaceResults (
    id BIGINT PRIMARY KEY,
    raceId BIGINT NOT NULL REFERENCES Races(id),
    driverId BIGINT NOT NULL REFERENCES Drivers(id),
    startPos INT NOT NULL,
    finishPos INT NOT NULL,
    hasFastestLap BOOLEAN NOT NULL,
    penalties INT NOT NULL,
    dnf BOOLEAN NOT NULL,
    sessionTime BIGINT NOT NULL,
    points INT NOT NULL,
    carDamage INT NOT NULL
);

CREATE TABLE Laps (
    id BIGINT PRIMARY KEY,
    raceResultId BIGINT NOT NULL REFERENCES RaceResults(id),
    position INT NOT NULL,
    tyreCompound INT NOT NULL,
    deltaToLeader INT NOT NULL,
    deltaToCarInFront INT NOT NULL,
    tyreWear INT NOT NULL,
    lapTime INT NOT NULL
);

CREATE TABLE Stints (
    id BIGINT PRIMARY KEY,  
    raceResultId BIGINT NOT NULL REFERENCES RaceResults(id),
    tyreCompound INT NOT NULL,
    startLap INT NOT NULL,
    endLap INT NOT NULL,
    pitStopTime INT NOT NULL
);

CREATE OR REPLACE FUNCTION update_driver_elo()
RETURNS TRIGGER AS $$
DECLARE
    difficultyMultiplier INT;
    sfDiff INT;
    wghtSfDiff FLOAT;
    wghtPoints FLOAT;
    wghtDmg FLOAT;
    wghtDnf FLOAT;
    wghtPen FLOAT;
    currentElo INT;
    deltaElo FLOAT;
    newElo INT;
    dmg INT;
    pointsTotal INT;
BEGIN
    SELECT aiDifficulty + raceLength
    INTO difficultyMultiplier
    FROM Races
    WHERE id = NEW.raceId;

    sfDiff := NEW.startPos - NEW.finishPos; 
    pointsTotal := NEW.points + CASE WHEN NEW.hasFastestLap THEN 1 ELSE 0 END;
    dmg := 100 - NEW.carDamage;

    wghtSfDiff := sfDiff * difficultyMultiplier;
    wghtPoints := pointsTotal * (difficultyMultiplier / 100.0);
    wghtDmg := dmg * (difficultyMultiplier / 100.0);
    wghtDnf := (CASE WHEN NEW.dnf THEN (20 - NEW.startPos) * difficultyMultiplier ELSE 0 END);
    wghtPen := NEW.penalties;

    SELECT ELO INTO currentElo FROM Drivers WHERE id = NEW.driverId;

    deltaElo := (wghtSfDiff + wghtPoints + wghtDmg - wghtDnf - wghtPen) / (currentElo / 10.0);
    newElo := currentElo + ROUND(deltaElo);

    UPDATE Drivers SET ELO = newElo WHERE id = NEW.driverId;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_driver_elo
AFTER INSERT ON RaceResults
FOR EACH ROW
EXECUTE FUNCTION update_driver_elo();

CREATE OR REPLACE FUNCTION update_driver_rank()
RETURNS TRIGGER AS $$
DECLARE
    newRankId INT;
BEGIN
    SELECT id INTO newRankId
    FROM Ranks
    WHERE 
        pointsMin <= NEW.ELO AND
        (pointsMax >= NEW.ELO OR pointsMax IS NULL)
    LIMIT 1;

    UPDATE Drivers
    SET rankId = newRankId
    WHERE id = NEW.id;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_rank_after_elo_update
AFTER UPDATE OF ELO ON Drivers
FOR EACH ROW
WHEN (OLD.ELO IS DISTINCT FROM NEW.ELO)
EXECUTE FUNCTION update_driver_rank();

INSERT INTO Ranks (id, name, icon, pointsMin, pointsMax) VALUES
(1, 'Bronze', '🥉', 0, 899),
(2, 'Silver', '🥈', 900, 1099),
(3, 'Gold', '🥇', 1100, 1399),
(4, 'Platinum', '💎', 1400, 1799),
(5, 'Master', '🧙‍♂️', 1800, 2499),
(6, 'Champion', '🏆', 2500, NULL);

INSERT INTO Tracks (id,name) VALUES
(1,'Melbeourne'),
(2,'Paul Ricard'),
(3,'Shanghai'),
(4,'Sakhir (Bahrain)'),
(5,'Catalunya'),
(6,'Monaco'),
(7,'Montreal'),
(8,'Silverstone'),
(9,'Hockenheim'),
(10,'Hungaroring'),
(11,'Spa'),
(12,'Monza'),
(13,'Singapore'),
(14,'Suzuka'),
(15,'Abu Dhabi'),
(16,'Texas'),
(17,'Brazil'),
(18,'Austria'),
(19,'Sochi'),
(20,'Mexico'),
(21,'Baku (Azerbajian)'),
(22,'Sakhir Short'),
(23,'Silverstone Short'),
(24,'Texas Short'),
(25,'Suzuka Short'),
(26,'Hanoi'),
(27,'Zandvoort'),
(28,'Imola'),
(29,'Portimão'),
(30,'Jeddah'),
(31,'Miami'),
(32,'Las Vegas'),
(33,'Losali');
