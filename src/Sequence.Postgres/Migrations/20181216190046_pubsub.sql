CREATE OR REPLACE FUNCTION notify_game_event_inserted()
RETURNS trigger AS $$
BEGIN
    PERFORM pg_notify('game_event_inserted', row_to_json(NEW)::TEXT);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER game_event_inserted
AFTER INSERT
ON public.game_event
FOR EACH ROW
EXECUTE PROCEDURE notify_game_event_inserted();
