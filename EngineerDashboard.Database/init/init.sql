\c telemetry;

CREATE TABLE Ranks (
    id SERIAL PRIMARY KEY,
    name VARCHAR(20) NOT NULL,
    icon TEXT NOT NULL,
    pointsMin INT NOT NULL,
    pointsMax INT
);

CREATE TABLE Drivers (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(48) NOT NULL,
    ELO INT NOT NULL DEFAULT 1000,
    rankId INT NOT NULL REFERENCES Ranks(id)
);

CREATE TABLE Tracks (
    id SERIAL PRIMARY KEY,
    name VARCHAR(20) NOT NULL
);

CREATE TABLE Races (
    id BIGSERIAL PRIMARY KEY,
    date DATE NOT NULL,
    trackId INT NOT NULL REFERENCES Tracks(id),
    aiDifficulty INT NOT NULL,
    raceLength INT NOT NULL
);

CREATE TABLE RaceResults (
    id BIGSERIAL PRIMARY KEY,
    raceId BIGINT NOT NULL REFERENCES Races(id),
    driverId BIGINT NOT NULL REFERENCES Drivers(id),
    startPos INT NOT NULL,
    finishPos INT NOT NULL,
    hasFastestLap BOOLEAN NOT NULL,
    penalties INT NOT NULL,
    dnf BOOLEAN NOT NULL,
    sessionTime FLOAT NOT NULL,
    points INT NOT NULL,
    carDamage INT NOT NULL
);

CREATE TABLE Laps (
    id BIGSERIAL PRIMARY KEY,
    raceResultId BIGINT NOT NULL REFERENCES RaceResults(id),
    position INT NOT NULL,
    tyreCompound INT NOT NULL,
    deltaToLeader INT NOT NULL,
    deltaToCarInFront INT NOT NULL,
    tyreWear FLOAT NOT NULL,
    lapTime INT NOT NULL
);

CREATE TABLE Stints (
    id BIGSERIAL PRIMARY KEY,  
    raceResultId BIGINT NOT NULL REFERENCES RaceResults(id),
    tyreCompound INT NOT NULL,
    startLap INT NOT NULL,
    endLap INT NOT NULL,
    pitStopTime INT NOT NULL
);

-- Funkcja aktualizująca ELO
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

-- Trigger
CREATE TRIGGER trg_update_driver_elo
AFTER INSERT ON RaceResults
FOR EACH ROW
EXECUTE FUNCTION update_driver_elo();


CREATE OR REPLACE FUNCTION update_driver_rank()
RETURNS TRIGGER AS $$
DECLARE
    newRankId INT;
BEGIN
    -- Znajdź odpowiedni rank na podstawie nowego ELO kierowcy
    SELECT id INTO newRankId
    FROM Ranks
    WHERE 
        pointsMin <= NEW.ELO AND
        (pointsMax >= NEW.ELO OR pointsMax IS NULL)
    LIMIT 1;

    -- Zaktualizuj rankId w Drivers
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
