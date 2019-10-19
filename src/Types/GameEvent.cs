using System.Collections.Immutable;

namespace Sequence
{
    public sealed class GameEvent
    {
        public GameEvent(
            PlayerId byPlayerId,
            Card? cardDrawn,
            Card cardUsed,
            Team? chip,
            Coord coord,
            int index,
            PlayerId? nextPlayerId,
            IImmutableList<Seq> sequences,
            Team? winner)
        {
            ByPlayerId = byPlayerId;
            CardDrawn = cardDrawn;
            CardUsed = cardUsed;
            Chip = chip;
            Coord = coord;
            Index = index;
            NextPlayerId = nextPlayerId;
            Sequences = sequences;
            Winner = winner;
        }

        public PlayerId ByPlayerId { get; }
        public Card? CardDrawn { get; }
        public Card CardUsed { get; }
        public Team? Chip { get; }
        public Coord Coord { get; }
        public int Index { get; }
        public PlayerId? NextPlayerId { get; }
        public IImmutableList<Seq> Sequences { get; }
        public Team? Winner { get; }
    }
}
