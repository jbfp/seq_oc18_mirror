using Moq;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Core.Test.CreateGameA
{
    public sealed class CreateGameHandlerTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "store",
                () => new CreateGameHandler(store: null)
            );
        }

        private readonly Mock<IGameStore> _store = new Mock<IGameStore>();
        private readonly CreateGameHandler _sut;

        public CreateGameHandlerTest()
        {
            _sut = new CreateGameHandler(_store.Object);
        }

        [Fact]
        public async Task CreateGameAsync_NullArgs()
        {
            var player1 = new PlayerId("player 1");
            var player2 = new PlayerId("player 2");

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "player1",
                testCode: () => _sut.CreateGameAsync(null, player2, CancellationToken.None)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "player2",
                testCode: () => _sut.CreateGameAsync(player1, null, CancellationToken.None)
            );
        }

        [Theory]
        [InlineData("123")]
        [InlineData(42)]
        [InlineData(true)]
        public async Task CreateGameAsync_ReturnsGameIdFromStore(object gameId)
        {
            var expected = new GameId(gameId);

            _store
                .Setup(s => s.PersistNewGameAsync(It.IsAny<NewGame>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var player1 = new PlayerId("Player 1");
            var player2 = new PlayerId("Player 2");

            var actual = await _sut.CreateGameAsync(player1, player2, CancellationToken.None);

            Assert.Equal(expected, actual);
        }
    }
}
