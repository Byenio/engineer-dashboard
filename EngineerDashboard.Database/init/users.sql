CREATE ROLE steward WITH LOGIN PASSWORD ']G1.00-R6wW,';

GRANT CONNECT ON DATABASE telemetry TO steward;

GRANT SELECT, UPDATE ON race_results TO steward;


CREATE ROLE telemetry_client WITH LOGIN PASSWORD '(456Jm928*?0';

GRANT CONNECT ON DATABASE telemetry TO telemetry_client;

GRANT SELECT, INSERT ON drivers, race_entries, races, race_results, laps, stops TO telemetry_client;


CREATE ROLE fia WITH LOGIN PASSWORD 'fh`>`2G1@20V';

GRANT CONNECT ON DATABASE telemetry TO fia;

GRANT SELECT, INSERT, UPDATE ON tracks, ranks, teams TO fia;


