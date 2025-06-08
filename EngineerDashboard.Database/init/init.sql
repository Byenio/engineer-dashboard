-- Teams table: Stores team information with manual IDs from C# enum
CREATE TABLE Teams (
                       id INTEGER PRIMARY KEY,
                       name VARCHAR(100) NOT NULL
);

-- Ranks table: Stores rank information with ELO ranges and icon
CREATE TABLE Ranks (
                       id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                       name VARCHAR(50) NOT NULL,
                       icon VARCHAR(255),
                       minPoints INTEGER NOT NULL,
                       maxPoints INTEGER,
                       CONSTRAINT check_points CHECK (minPoints <= maxPoints OR maxPoints IS NULL)
);

-- Drivers table: Stores driver information with manual IDs from game telemetry
CREATE TABLE Drivers (
                         id INTEGER PRIMARY KEY,
                         name VARCHAR(100) NOT NULL,
                         ELO INTEGER NOT NULL DEFAULT 1000,
                         rankId INTEGER NOT NULL DEFAULT 2,
                         teamId INTEGER,
                         FOREIGN KEY (rankId) REFERENCES Ranks(id) ON DELETE RESTRICT,
                         FOREIGN KEY (teamId) REFERENCES Teams(id) ON DELETE SET NULL
);

-- Tracks table: Stores track information with manual IDs from game telemetry
CREATE TABLE Tracks (
                        id INTEGER PRIMARY KEY,
                        name VARCHAR(100) NOT NULL
);

-- Races table: Stores race information
CREATE TABLE Races (
                       id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                       date TIMESTAMP NOT NULL,
                       trackId INTEGER NOT NULL,
                       AIDifficulty INTEGER NOT NULL CHECK (AIDifficulty >= 0 AND AIDifficulty <= 110),
                       raceLength INTEGER NOT NULL CHECK (raceLength > 0),
                       FOREIGN KEY (trackId) REFERENCES Tracks(id) ON DELETE RESTRICT
);

-- RaceEntries table: Junction table for driver-race participation
CREATE TABLE RaceEntries (
                             id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                             driverId INTEGER NOT NULL,
                             raceId INTEGER NOT NULL,
                             teamId INTEGER NOT NULL,
                             startPosition INTEGER NOT NULL CHECK (startPosition > 0),
                             FOREIGN KEY (driverId) REFERENCES Drivers(id) ON DELETE CASCADE,
                             FOREIGN KEY (raceId) REFERENCES Races(id) ON DELETE CASCADE,
                             FOREIGN KEY (teamId) REFERENCES Teams(id) ON DELETE SET NULL,
                             CONSTRAINT unique_driver_race UNIQUE (driverId, raceId)
);

-- RaceResults: Stores results of a race for a driver's race entry
CREATE TABLE RaceResults (
                            id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                            raceEntryId INTEGER NOT NULL,
                            finishPosition INTEGER NOT NULL CHECK (finishPosition > 0),
                            hasFastestLap BOOLEAN DEFAULT FALSE,
                            penaltiesInSeconds INTEGER NOT NULL,
                            hasDnf BOOLEAN DEFAULT FALSE,
                            points INTEGER NOT NULL,
                            averagedamage INTEGER NOT NULL CHECK (averagedamage BETWEEN 0 AND 100),
                            FOREIGN KEY (raceEntryId) REFERENCES RaceEntries(id) ON DELETE CASCADE
);

-- Laps table: Stores lap data for a driver's race entry
CREATE TABLE Laps (
                      id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                      raceEntryId INTEGER NOT NULL,
                      lapNum INTEGER NOT NULL CHECK (lapNum > 0),
                      currentPosition INTEGER NOT NULL CHECK (currentPosition > 0),
                      deltaToLeader INTEGER NOT NULL,
                      deltaToCarInFront INTEGER NOT NULL,
                      lastLapTime INTEGER NOT NULL,
                      tyreWear INTEGER NOT NULL CHECK (tyreWear >= 0 AND tyreWear <= 100),
                      FOREIGN KEY (raceEntryId) REFERENCES RaceEntries(id) ON DELETE CASCADE,
                      CONSTRAINT unique_race_entry_lap UNIQUE (raceEntryId, lapNum)
);

-- Stints table: Stores pit stop data (tyre info before pit stop)
CREATE TABLE Stints (
                        id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                        raceEntryId INTEGER NOT NULL,
                        endLap INTEGER NOT NULL CHECK (endLap > 0),
                        tyreCompound INTEGER NOT NULL,
                        tyreWear INTEGER NOT NULL CHECK (tyreWear >= 0 AND tyreWear <= 100),
                        pitStopTime INTEGER NOT NULL,
                        FOREIGN KEY (raceEntryId) REFERENCES RaceEntries(id) ON DELETE CASCADE,
                        CONSTRAINT unique_race_entry_endlap UNIQUE (raceEntryId, endLap)
);

-- Indexes for performance
CREATE INDEX idx_raceentries_driverid ON RaceEntries(driverId);
CREATE INDEX idx_raceentries_raceid ON RaceEntries(raceId);
CREATE INDEX idx_laps_raceentryid ON Laps(raceEntryId);
CREATE INDEX idx_stints_raceentryid ON Stints(raceEntryId);
CREATE INDEX idx_results_raceentryid ON RaceResults(raceEntryId);

-- Sample data for Ranks
INSERT INTO Ranks (name, icon, minPoints, maxPoints) VALUES
                                                         ('Bronze', '🥉', 0, 899),
                                                         ('Silver', '🥈', 900, 1099),
                                                         ('Gold', '🥇', 1100, 1399),
                                                         ('Platinum', '💎', 1400, 1799),
                                                         ('Master', '🧙‍♂️', 1800, 2499),
                                                         ('Champion', '🏆', 2500, NULL);

-- Sample data for Tracks (with manual IDs)
INSERT INTO Tracks (id, name) VALUES
                                  (0, 'Melbourne'),
                                  (1, 'Paul Ricard'),
                                  (2, 'Shanghai'),
                                  (3, 'Sakhir (Bahrain)'),
                                  (4, 'Catalunya'),
                                  (5, 'Monaco'),
                                  (6, 'Montreal'),
                                  (7, 'Silverstone'),
                                  (8, 'Hockenheim'),
                                  (9, 'Hungaroring'),
                                  (10, 'Spa'),
                                  (11, 'Monza'),
                                  (12, 'Singapore'),
                                  (13, 'Suzuka'),
                                  (14, 'Abu Dhabi'),
                                  (15, 'Texas'),
                                  (16, 'Brazil'),
                                  (17, 'Austria'),
                                  (18, 'Sochi'),
                                  (19, 'Mexico'),
                                  (20, 'Baku (Azerbajian)'),
                                  (21, 'Sakhir Short'),
                                  (22, 'Silverstone Short'),
                                  (23, 'Texas Short'),
                                  (24, 'Suzuka Short'),
                                  (25, 'Hanoi'),
                                  (26, 'Zandvoort'),
                                  (27, 'Imola'),
                                  (28, 'Portimao'),
                                  (29, 'Jeddah'),
                                  (30, 'Miami'),
                                  (31, 'Las Vegas'),
                                  (32, 'Losali');

-- Sample data for Teams (matching C# enum)
INSERT INTO Teams (id, name) VALUES
                                 (0, 'Mercedes'),
                                 (1, 'Ferrari'),
                                 (2, 'Red Bull'),
                                 (3, 'Williams'),
                                 (4, 'Aston Martin'),
                                 (5, 'Alpine'),
                                 (6, 'AlphaTauri'),
                                 (7, 'Haas'),
                                 (8, 'McLaren'),
                                 (9, 'Alfa Romeo'),
                                 (104, 'F1 World');

-- Trigger for ELO calculation
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
    startPosition INT;
    driverId INT;
BEGIN
    SELECT re.driverId, re.startPosition, r.AIDifficulty + r.raceLength
    INTO driverId, startPosition, difficultyMultiplier
    FROM RaceEntries re
             JOIN Races r ON r.id = re.raceId
    WHERE re.id = NEW.raceEntryId;

    -- Calculate start-finish difference
    sfDiff := startPosition - NEW.finishPosition;
    wghtSfDiff := sfDiff * difficultyMultiplier;

    -- Calculate weighted points
    wghtPoints := (NEW.points + CASE WHEN NEW.hasFastestLap THEN 1 ELSE 0 END) * (difficultyMultiplier / 100.0);

    -- Calculate weighted damage
    wghtDmg := (100 - NEW.averagedamage) * (difficultyMultiplier / 100.0);

    -- Calculate weighted DNF penalty
    wghtDnf := (CASE WHEN NEW.hasDnf THEN (20 - startPosition) * difficultyMultiplier ELSE 0 END);

    -- Calculate weighted penalties
    wghtPen := NEW.penaltiesInSeconds;

    -- Fetch current ELO
    SELECT ELO INTO currentElo FROM Drivers WHERE id = driverId;

    -- Calculate ELO change
    deltaElo := (wghtSfDiff + wghtPoints + wghtDmg - wghtDnf - wghtPen) / (currentElo / 10.0);
    newElo := currentElo + ROUND(deltaElo);

    -- Update Driver ELO
    UPDATE Drivers SET ELO = newElo WHERE id = driverId;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_driver_elo
    AFTER INSERT OR UPDATE OF finishPosition, hasFastestLap, penaltiesInSeconds, hasDnf, points, averagedamage
    ON RaceResults
    FOR EACH ROW
EXECUTE FUNCTION update_driver_elo();

-- Trigger for updating driver rank based on ELO
CREATE OR REPLACE FUNCTION update_driver_rank()
    RETURNS TRIGGER AS $$
DECLARE
    newRankId INT;
BEGIN
    SELECT id INTO newRankId
    FROM Ranks
    WHERE minPoints <= NEW.ELO AND (maxPoints >= NEW.ELO OR maxPoints IS NULL)
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