ALTER TABLE public.game_event
ADD COLUMN sequences sequence[] NOT NULL DEFAULT ARRAY[]::sequence[];

UPDATE public.game_event
SET sequences = (SELECT ARRAY[sequence])
WHERE sequence IS NOT NULL;

ALTER TABLE public.game_event
DROP COLUMN sequence;
