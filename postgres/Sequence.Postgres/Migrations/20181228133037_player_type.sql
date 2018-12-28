CREATE TYPE public.player_type AS ENUM ('user', 'bot');

ALTER TABLE public.game_player
ADD COLUMN player_type player_type NOT NULL DEFAULT 'user';

ALTER TABLE public.game_player
ALTER COLUMN player_type DROP DEFAULT;
