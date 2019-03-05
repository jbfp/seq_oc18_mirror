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
        AND gp.player_type = 'user'::player_type
    )
    SELECT
      g.game_id
    , (public.get_game_list_item_for_player(in_player_id, g.id)).*
    FROM game_ids AS g;
END
$$ LANGUAGE 'plpgsql' STABLE STRICT;
