using System;
using Xunit;

namespace Sequence.Core.CreateGame.Test
{
    public sealed class NewGameTest
    {
        private static readonly PlayerList _players = new PlayerList(
            new NewPlayer(
                new PlayerHandle("test 1"),
                PlayerType.User),
            new NewPlayer(
                    new PlayerHandle("test 2"),
                    PlayerType.User));
        private static readonly Seed _seed = new Seed(42);
        private static readonly BoardType _boardType = BoardType.OneEyedJack;

        [Fact]
        public void NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "players",
                testCode: () => new NewGame(players: null, _seed, _boardType)
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(42)]
        [InlineData(100)]
        public void BoardTypeMustBeDefined(int boardType)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                paramName: "boardType",
                testCode: () => new NewGame(_players, _seed, (BoardType)boardType)
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(42)]
        [InlineData(100)]
        public void NumSequencesToWinMustBeGreaterThanZeroAndLessThanFive(int numSequencesToWin)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                paramName: "numSequencesToWin",
                testCode: () => new NewGame(_players, _seed, _boardType, numSequencesToWin)
            );
        }
    }
}
