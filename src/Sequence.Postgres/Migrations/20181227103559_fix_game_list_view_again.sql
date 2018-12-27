CREATE FUNCTION public.get_game_list_item_for_player(
    IN in_player_id TEXT, IN in_game_id INTEGER,
    OUT next_player_id TEXT, OUT opponents TEXT[]
) AS $$
BEGIN
    next_player_id := (
        SELECT gp.player_id
        FROM
        (
            SELECT
                CASE WHEN ge.id IS NULL
                    THEN g.first_player_id
                    ELSE ge.next_player_id
                END AS id
            FROM public.game AS g

            LEFT JOIN (
                SELECT DISTINCT ON (game_id) * FROM public.game_event
                ORDER BY game_id ASC, idx DESC
            ) AS ge
            ON g.id = ge.game_id

            WHERE g.id = in_game_id
        ) AS next_player

        INNER JOIN public.game_player AS gp
        ON gp.id = next_player.id
    );

    opponents := ARRAY(
        SELECT gp.player_id
        FROM public.game_player AS gp
        WHERE gp.game_id = in_game_id
        AND gp.player_id <> in_player_id
    );
END
$$ LANGUAGE 'plpgsql' STABLE STRICT;

DROP FUNCTION public.get_game_list_for_player;

CREATE FUNCTION public.get_game_list_for_player(IN in_player_id TEXT)
RETURNS TABLE (game_id UUID, next_player_id TEXT, opponents TEXT[])
AS $$
BEGIN
    RETURN QUERY WITH game_ids AS (
        SELECT g.id, g.game_id
        FROM public.game AS g
        INNER JOIN public.game_player AS gp
        ON gp.game_id = g.id
        WHERE gp.player_id = in_player_id
    )
    SELECT
      g.game_id
    , (public.get_game_list_item_for_player(in_player_id, g.id)).*
    FROM game_ids AS g;
END
$$ LANGUAGE 'plpgsql' STABLE STRICT;
