using System;

namespace Sequence.Core.CreateGame
{
    public sealed class NewGame
    {
        public NewGame(PlayerList players, Seed seed, BoardType boardType)
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
        }

        public PlayerList PlayerList { get; }
        public Seed Seed { get; }
        public BoardType BoardType { get; }
    }
}
