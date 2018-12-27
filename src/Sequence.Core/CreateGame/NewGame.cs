using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.Core.CreateGame
{
    public sealed class NewGame
    {
        public NewGame(IImmutableList<PlayerId> players, PlayerId firstPlayerId, Seed seed)
        {
            Players = players ?? throw new ArgumentNullException(nameof(players));
            FirstPlayerId = firstPlayerId ?? throw new ArgumentNullException(nameof(firstPlayerId));
            Seed = seed;

            if (Players.First().Equals(Players.Last()))
            {
                throw new ArgumentException("Player 1 and Player 2 must not be the same player.");
            }
        }

        public IImmutableList<PlayerId> Players { get; }
        public PlayerId FirstPlayerId { get; }
        public Seed Seed { get; }
    }
}
