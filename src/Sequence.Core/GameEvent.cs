namespace Sequence.Core
{
    public sealed class GameEvent
    {
        public PlayerId ByPlayerId { get; set; }
        public Card CardDrawn { get; set; }
        public Card CardUsed { get; set; }
        public Team? Chip { get; set; }
        public Coord Coord { get; set; }
        public int Index { get; set; }
        public PlayerId NextPlayerId { get; set; }
        public Seq Sequence { get; set; }
    }
}
