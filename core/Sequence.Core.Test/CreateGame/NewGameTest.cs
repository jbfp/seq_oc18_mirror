using System;
using Xunit;

namespace Sequence.Core.CreateGame.Test
{
    public sealed class NewGameTest
    {
        [Fact]
        public void NullArgs()
        {
            var seed = new Seed(42);
            var boardType = BoardType.OneEyedJack;

            Assert.Throws<ArgumentNullException>(
                paramName: "players",
                testCode: () => new NewGame(players: null, seed, boardType)
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(42)]
        [InlineData(100)]
        public void BoardTypeMustBeDefined(int boardType)
        {
            var players = new PlayerList(
                new NewPlayer(
                    new PlayerHandle("test 1"),
                    PlayerType.User),
                new NewPlayer(
                        new PlayerHandle("test 2"),
                        PlayerType.User));

            var seed = new Seed(42);

            Assert.Throws<ArgumentOutOfRangeException>(
                paramName: "boardType",
                testCode: () => new NewGame(players, seed, (BoardType)boardType)
            );
        }
    }
}
