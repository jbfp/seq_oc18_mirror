ALTER TABLE public.game
ADD COLUMN num_sequences_to_win INTEGER NOT NULL DEFAULT 1;

ALTER TABLE public.game
ALTER COLUMN num_sequences_to_win DROP DEFAULT;
