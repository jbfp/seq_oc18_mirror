ALTER TABLE public.game
ADD COLUMN first_player_id TEXT NULL;

UPDATE public.game SET first_player_id = player1;

ALTER TABLE public.game
ALTER COLUMN first_player_id SET NOT NULL;

DROP FUNCTION public.get_game_list_for_player;

CREATE FUNCTION public.get_game_list_for_player(IN player_id TEXT)
RETURNS TABLE (game_id UUID, next_player_id TEXT, opponent TEXT)
AS $$
BEGIN
    RETURN QUERY SELECT
      g.game_id AS game_id
    , CASE WHEN ge.id IS NULL THEN g.first_player_id ELSE ge.next_player_id END AS next_player_id
    , CASE WHEN player_id = g.player1 THEN g.player2 ELSE g.player1 END AS opponent
    FROM public.game AS g

    LEFT JOIN (
        SELECT DISTINCT ON (game_id) * FROM public.game_event
        ORDER BY game_id ASC, idx DESC
    ) AS ge
    ON g.id = ge.game_id

    WHERE g.player1 = player_id
       OR g.player2 = player_id

    ORDER BY ge.timestamp DESC;
END
$$ LANGUAGE 'plpgsql' STABLE STRICT;

