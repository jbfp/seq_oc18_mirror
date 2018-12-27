CREATE TABLE public.game_player
( id BIGSERIAL NOT NULL
, game_id INTEGER NOT NULL
, player_id TEXT NOT NULL
, PRIMARY KEY (id)
, FOREIGN KEY (game_id) REFERENCES public.game (id) ON DELETE CASCADE
, UNIQUE (game_id, player_id)
);

INSERT INTO public.game_player (game_id, player_id)
SELECT id, unnest(ARRAY[player1, player2]) FROM public.game;

CREATE INDEX ix_game_player
ON public.game_player (player_id)
INCLUDE (game_id);

ALTER TABLE public.game
DROP COLUMN player1,
DROP COLUMN player2;

-- change first_player_id to foreign key into game_player table.
ALTER TABLE public.game
ADD COLUMN first_player_id_dummy INTEGER NULL;

UPDATE public.game AS g
SET first_player_id_dummy = (
    SELECT id
    FROM public.game_player AS gp
    WHERE gp.game_id = g.id
    AND gp.player_id = g.first_player_id
);

ALTER TABLE public.game
DROP COLUMN first_player_id;

ALTER TABLE public.game
RENAME COLUMN first_player_id_dummy TO first_player_id;

ALTER TABLE public.game
ADD FOREIGN KEY (first_player_id) REFERENCES public.game_player (id);

-- change by_player_id to foreign key into game_player table.
ALTER TABLE public.game_event
ADD COLUMN by_player_id_dummy INTEGER NULL;

UPDATE public.game_event AS ge
SET by_player_id_dummy = (
    SELECT id
    FROM public.game_player AS gp
    WHERE gp.game_id = ge.game_id
    AND gp.player_id = ge.by_player_id
);

ALTER TABLE public.game_event
DROP COLUMN by_player_id;

ALTER TABLE public.game_event
RENAME COLUMN by_player_id_dummy TO by_player_id;

ALTER TABLE public.game_event
ALTER COLUMN by_player_id SET NOT NULL,
ADD FOREIGN KEY (by_player_id) REFERENCES public.game_player (id);

-- change next_player_id to foreign key into game_player table.
ALTER TABLE public.game_event
ADD COLUMN next_player_id_dummy INTEGER NULL;

UPDATE public.game_event AS ge
SET next_player_id_dummy = (
    SELECT id
    FROM public.game_player AS gp
    WHERE gp.game_id = ge.game_id
    AND gp.player_id = ge.next_player_id
)
WHERE ge.next_player_id IS NOT NULL;

ALTER TABLE public.game_event
DROP COLUMN next_player_id;

ALTER TABLE public.game_event
RENAME COLUMN next_player_id_dummy TO next_player_id;

ALTER TABLE public.game_event
ADD FOREIGN KEY (next_player_id) REFERENCES public.game_player (id);

-- update get_game_list_for_player function.
DROP FUNCTION public.get_game_list_for_player;

CREATE FUNCTION public.get_game_list_for_player(IN player_id TEXT)
RETURNS TABLE (game_id UUID, next_player_id TEXT, opponent TEXT)
AS $$
BEGIN
    RETURN QUERY SELECT
      g.game_id AS game_id
    , NULL AS next_player_id
    , '' AS opponent
    FROM public.game AS g

    INNER JOIN public.game_player AS gp
    ON gp.game_id = g.id

    WHERE gp.player_id = @player_id;
END
$$ LANGUAGE 'plpgsql' STABLE STRICT;

