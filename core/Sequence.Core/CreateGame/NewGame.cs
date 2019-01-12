using System;

namespace Sequence.Core.CreateGame
{
    public sealed class NewGame
    {
        public const int MaxNumberOfSequencesToWin = 4;

        public NewGame(PlayerList players, Seed seed, BoardType boardType, int numSequencesToWin = 1)
        {
            PlayerList = players ?? throw new ArgumentNullException(nameof(players));
            Seed = seed;

            if (Enum.IsDefined(typeof(BoardType), boardType))
            {
                BoardType = boardType;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(boardType), boardType, null);
            }

            if (numSequencesToWin > 0 && numSequencesToWin <= MaxNumberOfSequencesToWin)
            {
                NumberOfSequencesToWin = numSequencesToWin;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(numSequencesToWin), numSequencesToWin, null);
            }
        }

        public PlayerList PlayerList { get; }
        public Seed Seed { get; }
        public BoardType BoardType { get; }
        public int NumberOfSequencesToWin { get; }
    }
}
