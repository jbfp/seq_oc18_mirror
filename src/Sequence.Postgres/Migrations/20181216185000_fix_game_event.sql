-- Drop primary key.
ALTER TABLE public.game_event DROP CONSTRAINT game_event_pkey;

-- Add 'id' column.
ALTER TABLE public.game_event ADD COLUMN id SERIAL NOT NULL;

-- Set 'id' to be the new primary key.
ALTER TABLE public.game_event ADD PRIMARY KEY (id);
