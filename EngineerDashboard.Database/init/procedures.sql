CREATE OR REPLACE PROCEDURE set_penalty(race_result_id INT, new_penalty INT)
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM race_results WHERE id = race_result_id) THEN
        UPDATE race_results
        SET penalties = new_penalty
        WHERE id = result_id;
    ELSE
        RAISE NOTICE 'Nonexistent ID --> %', race_result_id
    END IF;
END;
$$;


CREATE OR REPLACE PROCEDURE change_position(race_result_id INT, new_position INT)
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM race_results WHERE id = race_result_id) THEN
        UPDATE race_results
        SET finish_position = new_position
        WHERE id = race_result_id;
    ELSE
        RAISE NOTICE 'Nonexistent ID --> %', race_result_id
    END IF;
END;
$$;

