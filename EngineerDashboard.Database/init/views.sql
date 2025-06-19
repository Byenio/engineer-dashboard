CREATE VIEW drivers_table AS
SELECT drivers.id, drivers.name, teams.name, drivers.elo, ranks.name
FROM drivers
JOIN teams ON teams.id = drivers.team_id
JOIN ranks ON ranks.id = drivers.rank_id;

CREATE VIEW races_2025 AS
SELECT races.id, races.date, tracks.name, races.ai_difficulty
FROM races
JOIN tracks ON tracks.id = races.track_id
WHERE EXTRACT(YEAR FROM races.date) = EXTRACT(YEAR FROM CURRENT_DATE);

CREATE VIEW drivers_bronze AS
SELECT drivers.id, drivers.name, drivers.elo
FROM drivers
JOIN ranks ON ranks.id = drivers.rank_id
WHERE ranks.name = 'Bronze';

CREATE VIEW drivers_silver AS
SELECT drivers.id, drivers.name, drivers.elo
FROM drivers
JOIN ranks ON ranks.id = drivers.rank_id
WHERE ranks.name = 'Silver';

CREATE VIEW drivers_gold AS
SELECT drivers.id, drivers.name, drivers.elo
FROM drivers
JOIN ranks ON ranks.id = drivers.rank_id
WHERE ranks.name = 'Gold';

CREATE VIEW drivers_platinum AS
SELECT drivers.id, drivers.name, drivers.elo
FROM drivers
JOIN ranks ON ranks.id = drivers.rank_id
WHERE ranks.name = 'Platinum';

CREATE VIEW drivers_master AS
SELECT drivers.id, drivers.name, drivers.elo
FROM drivers
JOIN ranks ON ranks.id = drivers.rank_id
WHERE ranks.name = 'Master';

CREATE VIEW drivers_champion AS
SELECT drivers.id, drivers.name, drivers.elo
FROM drivers
JOIN ranks ON ranks.id = drivers.rank_id
WHERE ranks.name = 'Champion';