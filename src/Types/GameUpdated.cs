using System.Collections.Immutable;

namespace Sequence
{
    public sealed class GameUpdated
    {
        public GameUpdated(IImmutableList<IGameEvent> gameEvents, int version)
        {
            GameEvents = gameEvents;
            Version = version;
        }

        public IImmutableList<IGameEvent> GameEvents { get; }
        public int Version { get; }
    }
}
