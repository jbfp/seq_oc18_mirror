using System;
using System.Collections.Immutable;

namespace Sequence.Core
{
    public sealed class GameInit
    {
        public GameInit(IImmutableList<PlayerId> players, PlayerId firstPlayer, Seed seed)
        {
            Players = players ?? throw new ArgumentNullException(nameof(players));
            FirstPlayer = firstPlayer ?? throw new ArgumentNullException(nameof(firstPlayer));
            Seed = seed;
        }

        public IImmutableList<PlayerId> Players { get; }
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
