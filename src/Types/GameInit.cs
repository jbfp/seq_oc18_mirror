using System;
using System.Collections.Immutable;

namespace Sequence
{
    public sealed class GameInit
    {
        public GameInit(
            IImmutableList<Player> players,
            PlayerId firstPlayerId,
            Seed seed,
            BoardType boardType,
            int numSequencesToWin)
        {
            Players = players;
            FirstPlayerId = firstPlayerId;
            Seed = seed;
            BoardType = boardType;
            NumberOfSequencesToWin = numSequencesToWin;
        }

        public IImmutableList<Player> Players { get; }
        public PlayerId FirstPlayerId { get; }
        public Seed Seed { get; }
        public BoardType BoardType { get; }
        public int NumberOfSequencesToWin { get; }
    }
}
