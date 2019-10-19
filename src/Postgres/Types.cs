using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.Postgres
{
#pragma warning disable CS0649, CS8618
    internal sealed class GameEventRow
    {
        public GameId surrogate_game_id;
        public int game_id;
        public int idx;
        public CardComposite? card_drawn;
        public CardComposite card_used;
        public Team? chip;
        public CoordComposite coord;
        public int id;
        public DateTimeOffset timestamp;
        public SequenceComposite[] sequences;
        public PlayerId by_player_id;
        public PlayerId? next_player_id;
        public Team? winner;

        public GameEvent ToGameEvent() => ToGameEvent(this);

        public static GameEvent ToGameEvent(GameEventRow row)
        {
            return new GameEvent(
                byPlayerId: row.by_player_id,
                cardDrawn: row.card_drawn?.ToCard(),
                cardUsed: row.card_used.ToCard(),
                chip: row.chip,
                coord: row.coord.ToCoord(),
                index: row.idx,
                nextPlayerId: row.next_player_id,
                sequences: row.sequences.Select(SequenceComposite.ToSequence).ToImmutableArray(),
                winner: row.winner);
        }
    }
#pragma warning restore CS0649, CS8618
}
