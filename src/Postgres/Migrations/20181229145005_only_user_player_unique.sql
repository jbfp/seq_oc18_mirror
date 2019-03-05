ALTER TABLE public.game_player
DROP CONSTRAINT game_player_game_id_player_id_key;

CREATE UNIQUE INDEX game_player_game_id_player_id_key
ON public.game_player (game_id, player_id)
WHERE player_type = 'user'::player_type;
