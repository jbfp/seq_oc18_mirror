ALTER TABLE public.game_event

DROP CONSTRAINT game_event_by_player_id_fkey,
DROP CONSTRAINT game_event_next_player_id_fkey,

ADD CONSTRAINT game_event_by_player_id_fkey
FOREIGN KEY (by_player_id)  REFERENCES public.game_player (id) ON DELETE CASCADE,

ADD CONSTRAINT game_event_next_player_id_fkey
FOREIGN KEY (next_player_Id) REFERENCES public.game_player (id) ON DELETE CASCADE;
