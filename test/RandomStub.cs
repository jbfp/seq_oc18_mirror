using System;

namespace Sequence.Test
{
    internal sealed class RandomStub : Random
    {
        private readonly int _value;

        public RandomStub(int value) => _value = value;

        protected override double Sample() => _value;
    }
}
