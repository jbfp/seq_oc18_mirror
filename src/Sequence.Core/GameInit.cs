using System;

namespace Sequence.Core
{
    public sealed class GameInit
    {
        public GameInit(PlayerId player1, PlayerId player2, PlayerId firstPlayer, Seed seed)
        {
            Player1 = player1 ?? throw new ArgumentNullException(nameof(player1));
            Player2 = player2 ?? throw new ArgumentNullException(nameof(player2));
            FirstPlayer = firstPlayer ?? throw new ArgumentNullException(nameof(firstPlayer));
            Seed = seed;
        }

        public PlayerId Player1 { get; }
        public PlayerId Player2 { get; }
        public PlayerId FirstPlayer { get; }
        public Seed Seed { get; }
    }

    public struct Seed
    {
        private readonly int _value;

        public Seed(int value)
        {
            _value = value;
        }

        public Random ToRandom() => new Random(_value);

        public int ToInt32() => _value;
    }
}
