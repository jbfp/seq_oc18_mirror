using System;

namespace Sequence.Core.Bots
{
    [Bot(Name)]
    public sealed class RandomBot : IBot
    {
        public const string Name = "Random Bot";

        private readonly Random _rng;

        public RandomBot() => _rng = new Random();
        public RandomBot(int seed) => _rng = new Random(seed);
    }
}
