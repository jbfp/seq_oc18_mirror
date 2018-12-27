ALTER TABLE public.game_event
ADD CONSTRAINT game_event_game_id_fkey
FOREIGN KEY (game_id)  REFERENCES public.game (id) ON DELETE CASCADE
