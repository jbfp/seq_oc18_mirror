namespace Sequence.GetGameEvents
{
    public sealed class Player
    {
        public PlayerHandle Handle { get; set; }
        public PlayerId Id { get; set; }
        public int NumberOfCards { get; set; }
        public Team Team { get; set; }
        public PlayerType Type { get; set; }
    }
}
