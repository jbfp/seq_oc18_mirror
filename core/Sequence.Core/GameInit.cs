using System;
using System.Collections.Immutable;

namespace Sequence.Core
{
    public sealed class GameInit
    {
        public GameInit(IImmutableList<Player> players, PlayerId firstPlayerId, Seed seed, BoardType boardType)
        {
            Players = players ?? throw new ArgumentNullException(nameof(players));
            FirstPlayerId = firstPlayerId ?? throw new ArgumentNullException(nameof(firstPlayerId));
            Seed = seed;
            BoardType = boardType;
        }

        public IImmutableList<Player> Players { get; }
        public PlayerId FirstPlayerId { get; }
        public Seed Seed { get; }
        public BoardType BoardType { get; }
    }
}
