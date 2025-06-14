CREATE TABLE teams (
    id INTEGER PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE ranks (
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(50) NOT NULL,
    icon VARCHAR(255),
    min_points INTEGER NOT NULL,
    max_points INTEGER,
    CONSTRAINT check_points CHECK (min_points <= max_points OR max_points IS NULL)
);

CREATE TABLE drivers (
    id INTEGER PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    elo INTEGER NOT NULL DEFAULT 1000,
    rank_id INTEGER NOT NULL DEFAULT 2,
    team_id INTEGER,
    FOREIGN KEY (rank_id) REFERENCES ranks(id) ON DELETE RESTRICT,
    FOREIGN KEY (team_id) REFERENCES teams(id) ON DELETE SET NULL
);

CREATE TABLE tracks (
    id INTEGER PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE races (
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    date TIMESTAMP NOT NULL,
    track_id INTEGER NOT NULL,
    ai_difficulty INTEGER NOT NULL CHECK (ai_difficulty >= 0 AND ai_difficulty <= 110),
    length INTEGER NOT NULL CHECK (length > 0),
    FOREIGN KEY (track_id) REFERENCES tracks(id) ON DELETE RESTRICT
);

CREATE TABLE race_entries (
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    driver_id INTEGER NOT NULL,
    race_id INTEGER NOT NULL,
    FOREIGN KEY (driver_id) REFERENCES drivers(id) ON DELETE CASCADE,
    FOREIGN KEY (race_id) REFERENCES races(id) ON DELETE CASCADE,
    CONSTRAINT unique_driver_race UNIQUE (driver_id, race_id)
);

CREATE TABLE race_results (
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    race_entry_id INTEGER NOT NULL,
    start_position INTEGER NOT NULL CHECK (start_position > 0),
    finish_position INTEGER NOT NULL CHECK (finish_position > 0),
    has_fastest_lap BOOLEAN DEFAULT FALSE,
    points INTEGER NOT NULL,
    penalties INTEGER NOT NULL,
    damage INTEGER NOT NULL CHECK (damage BETWEEN 0 AND 100),
    dnf BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (race_entry_id) REFERENCES race_entries(id) ON DELETE CASCADE
);

CREATE TABLE tyre_compounds (
    id INTEGER PRIMARY KEY,
    name VARCHAR(10) NOT NULL
);

CREATE TABLE laps (
    id               INTEGER GENERATED ALWAYS AS IDENTITY,
    race_entry_id    INTEGER NOT NULL,
    lap_number       INTEGER NOT NULL CHECK (lap_number > 0),
    tyre_wear        INTEGER NOT NULL CHECK (tyre_wear >= 0 AND tyre_wear <= 100),
    tyre_compound_id INTEGER NOT NULL,
    current_position INTEGER NOT NULL CHECK (current_position > 0),
    delta_leader     INTEGER NOT NULL CHECK (delta_leader >= 0),
    delta_front      INTEGER NOT NULL CHECK (delta_front >= 0),
    last_lap_time    INTEGER NOT NULL,
    CONSTRAINT unique_race_entry_lap UNIQUE (race_entry_id, lap_number),
    FOREIGN KEY (race_entry_id) REFERENCES race_entries (id),
    FOREIGN KEY (tyre_compound_id) REFERENCES tyre_compounds (id) ON DELETE CASCADE
);

CREATE TABLE pit_stops (
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    race_entry_id INTEGER NOT NULL,
    lap_number INTEGER NOT NULL CHECK (lap_number > 0),
    pit_stop_time INTEGER NOT NULL,
    FOREIGN KEY (race_entry_id) REFERENCES race_entries(id) ON DELETE CASCADE,
    CONSTRAINT unique_race_entry_stop UNIQUE (race_entry_id, lap_number)
);

CREATE INDEX idx_raceentries_driverid ON race_entries(driver_id);
CREATE INDEX idx_raceentries_raceid ON race_entries(race_id);
CREATE INDEX idx_laps_raceentryid ON laps(race_entry_id);
CREATE INDEX idx_pitstops_raceentryid ON pit_stops(race_entry_id);
CREATE INDEX idx_results_raceentryid ON race_results(race_entry_id);

INSERT INTO ranks (name, icon, min_points, max_points) VALUES
    ('Bronze', '🥉', 0, 899),
    ('Silver', '🥈', 900, 1099),
    ('Gold', '🥇', 1100, 1399),
    ('Platinum', '💎', 1400, 1799),
    ('Master', '🧙‍♂️', 1800, 2499),
    ('Champion', '🏆', 2500, NULL);

INSERT INTO tracks (id, name) VALUES
    (0, 'Melbourne'),
    (1, 'Paul Ricard'),
    (2, 'Shanghai'),
    (3, 'Bahrain'),
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
    (20, 'Azerbajian'),
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

INSERT INTO teams (id, name) VALUES
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

INSERT INTO tyre_compounds(id, name) VALUES
    (16, 'SOFT'),
    (17, 'MEDIUM'),
    (18, 'HARD'),
    (7, 'INTER'),
    (8, 'WET');

CREATE OR REPLACE FUNCTION update_driver_elo()
    RETURNS TRIGGER AS $$
DECLARE
    difficulty_multiplier INT;
    start_finish_difference INT;
    weighted_start_finish_difference FLOAT;
    weighted_points FLOAT;
    weighted_dmg FLOAT;
    weighted_dnf FLOAT;
    weighted_pen FLOAT;
    current_elo INT;
    delta_elo FLOAT;
    new_elo INT;
    driver_id INT;
BEGIN
    SELECT re.driver_id, r.ai_difficulty + r.length
    INTO driver_id, difficulty_multiplier
    FROM race_entries re
             JOIN races r ON r.id = re.race_id
    WHERE re.id = NEW.race_entry_id;

    start_finish_difference := NEW.start_position - NEW.finish_position;
    weighted_start_finish_difference := start_finish_difference * difficulty_multiplier;

    weighted_points := (NEW.points + CASE WHEN NEW.has_fastest_lap THEN 1 ELSE 0 END) * (difficulty_multiplier / 100.0);

    weighted_dmg := (100 - NEW.damage) * (difficulty_multiplier / 100.0);

    weighted_dnf := (CASE WHEN NEW.dnf THEN (20 - NEW.start_position) * difficulty_multiplier ELSE 0 END);

    weighted_pen := NEW.penalties;

    SELECT elo INTO current_elo FROM drivers WHERE id = driver_id;
    
    delta_elo := (weighted_start_finish_difference + weighted_points + weighted_dmg - weighted_dnf - weighted_pen) / GREATEST(current_elo / 10.0, 1.0);
    new_elo := current_elo + ROUND(delta_elo);

    UPDATE drivers SET elo = new_elo WHERE id = driver_id;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trg_update_driver_elo
    AFTER INSERT OR UPDATE OF start_position, finish_position, has_fastest_lap, points, penalties, damage, dnf
    ON race_results
    FOR EACH ROW
EXECUTE FUNCTION update_driver_elo();

CREATE OR REPLACE FUNCTION update_driver_rank()
    RETURNS TRIGGER AS $$
DECLARE
    new_rank_id INT;
BEGIN
    SELECT id INTO new_rank_id
    FROM ranks
    WHERE min_points <= NEW.elo AND (max_points >= NEW.elo OR max_points IS NULL)
    LIMIT 1;

    UPDATE drivers
    SET rank_id = new_rank_id
    WHERE id = NEW.id;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trg_update_rank_after_elo_update
    AFTER UPDATE OF elo ON drivers
    FOR EACH ROW
    WHEN (OLD.elo IS DISTINCT FROM NEW.elo)
EXECUTE FUNCTION update_driver_rank();