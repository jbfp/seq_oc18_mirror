using System;
using Xunit;

namespace Sequence.Core.Test
{
    public sealed partial class GameTest
    {
        [Fact]
        public void GetMovesForPlayer_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "playerId",
                testCode: () => _sut.GetMovesForPlayer(playerId: null)
            );
        }
    }
}
