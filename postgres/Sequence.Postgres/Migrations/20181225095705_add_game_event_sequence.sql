CREATE TYPE public.sequence AS
( team chip
, coords coord ARRAY
);

ALTER TABLE public.game_event
ADD COLUMN sequence sequence NULL;
