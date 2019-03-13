namespace Sequence
{
    public sealed class GameUpdated
    {
        public IGameEvent[] GameEvents { get; set; }
        public int Version { get; set; }
    }
}
