CREATE TABLE public.simulation
( game_id INTEGER NOT NULL
, created_by TEXT NOT NULL
, PRIMARY KEY (game_id)
, FOREIGN KEY (game_id) REFERENCES public.game (id)
);

CREATE INDEX ix_simulation_created_by
ON public.simulation (created_by)
INCLUDE (game_id);
