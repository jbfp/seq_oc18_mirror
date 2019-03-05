ALTER TABLE public.game_event
ADD COLUMN winner chip NULL;

UPDATE public.game_event
SET winner = sub.team
FROM (
    SELECT id, (sequence).team
    FROM public.game_event
    WHERE sequence IS NOT NULL
) AS sub
WHERE game_event.id = sub.id;
