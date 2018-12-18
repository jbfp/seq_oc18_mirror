CREATE OR REPLACE FUNCTION public.get_game_list_for_player(IN player_id TEXT)
RETURNS TABLE (game_id UUID, next_player_id TEXT)
AS $$
BEGIN
    RETURN QUERY SELECT
      g.game_id
    , COALESCE(ge.next_player_id, g.player1)
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
