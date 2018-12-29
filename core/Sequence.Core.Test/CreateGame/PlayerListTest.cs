using Sequence.Core.CreateGame;
using System;
using System.Linq;
using Xunit;

namespace Sequence.Core.Test.CreateGame
{
    public sealed class PlayerListTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(() => new PlayerList(players: null));
        }

        [Fact]
        public void Constructor_FailsIfAnyPlayersAreSame()
        {
            var player = TestPlayer.Get;

            Assert.Throws<ArgumentException>(() =>
                new PlayerList(player, player));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(7)]
        [InlineData(100)]
        [InlineData(42)]
        public void Constructor_OnlyCertainGamesSizesAllowed(int size)
        {
            var players = Enumerable
                .Range(0, size)
                .Select(_ => TestPlayer.Get)
                .ToArray();

            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerList(players));
        }
    }
}
