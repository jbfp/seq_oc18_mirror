using System;
using Xunit;

namespace Sequence.Core.CreateGame.Test
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
            Assert.Throws<ArgumentException>(() =>
                new PlayerList(
                    new PlayerId("player"),
                    new PlayerId("player")));
        }
    }
}
