using System;

namespace Sequence.Core.Bots
{
    [Bot(Name)]
    public sealed class NullBot : IBot
    {
        public const string Name = "Null Bot";

        public (Card, Coord) Decide(GameView game) => throw new NotSupportedException();
    }
}
