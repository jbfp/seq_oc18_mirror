using Sequence.Bots;
using System;
using Xunit;

namespace Sequence.Test.Bots
{
    public sealed partial class GameTest
    {
        [Fact]
        public void GetMovesForPlayer_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "view",
                testCode: () => Moves.FromGameView(view: null)
            );
        }
    }
}
