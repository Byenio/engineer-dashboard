CREATE OR REPLACE FUNCTION get_driver_races(driver INT)
    RETURNS TABLE (
                      race_date TIMESTAMP,
                      track_name VARCHAR(100),
                      finish_position INT,
                      has_fastest_lap BOOLEAN,
                      dnf BOOLEAN,
                      penalties INT,
                      points INT
                  )
    LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
        SELECT
            races.date,
            tracks.name,
            race_results.finish_position,
            race_results.has_fastest_lap,
            race_results.dnf,
            race_results.penalties,
            race_results.points
        FROM race_results
                 JOIN race_entries ON race_entries.id = race_results.race_entry_id
                 JOIN races ON races.id = race_entries.race_id
                 JOIN tracks ON tracks.id = races.track_id
        WHERE race_entries.driver_id = driver;
END;
$$;