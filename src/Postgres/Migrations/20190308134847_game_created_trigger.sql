CREATE OR REPLACE FUNCTION notify_game_inserted()
RETURNS trigger AS $$
BEGIN
    PERFORM pg_notify('game_inserted', row_to_json(NEW)::TEXT);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER game_inserted
AFTER INSERT
ON public.game
FOR EACH ROW
EXECUTE PROCEDURE notify_game_inserted();
