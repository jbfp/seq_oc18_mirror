CREATE OR REPLACE FUNCTION public.notify_game_event_inserted()
RETURNS trigger
LANGUAGE plpgsql
AS $function$
DECLARE
    surrogate_game_id UUID;
    output_row RECORD;
BEGIN
    SELECT game_id INTO surrogate_game_id FROM public.game WHERE id = NEW.game_id;
    SELECT surrogate_game_id, NEW.* INTO output_row;
    PERFORM pg_notify('game_event_inserted', row_to_json(output_row)::TEXT);
    RETURN NEW;
END;
$function$
