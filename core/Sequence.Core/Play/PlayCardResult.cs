namespace Sequence.Core.Play
{
    public sealed class PlayCardResult
    {
        public Card CardDrawn { get; set; }
        public Card CardUsed { get; set; }
        public Team? Chip { get; set; }
        public Coord Coord { get; set; }
        public PlayerId NextPlayerId { get; set; }
        public int Version { get; set; }
    }
}
