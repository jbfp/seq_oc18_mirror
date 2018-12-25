using Moq;
using Sequence.Core;
using Sequence.Core.GetGame;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Core.GetGame
{
    public sealed class GetGameHandlerTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "provider",
                () => new GetGameHandler(provider: null)
            );
        }

        private readonly Mock<IGameProvider> _provider = new Mock<IGameProvider>();
        private readonly GetGameHandler _sut;

        public GetGameHandlerTest()
        {
            _sut = new GetGameHandler(_provider.Object);
        }

        [Fact]
        public async Task ThrowsWhenArgsAreNull()
        {
            var gameId = new GameId("dummy");
            var playerId = new PlayerId("dummy");

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.GetGameViewForPlayerAsync(null, playerId, CancellationToken.None)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "playerId",
                testCode: () => _sut.GetGameViewForPlayerAsync(gameId, null, CancellationToken.None)
            );
        }

        [Fact]
        public async Task ThrowsWhenCanceled()
        {
            var gameId = new GameId("dummy");
            var playerId = new PlayerId("dummy");
            var cancellationToken = new CancellationToken(canceled: true);

            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: () => _sut.GetGameViewForPlayerAsync(gameId, playerId, cancellationToken)
            );
        }

        [Fact]
        public async Task ThrowsIfGameDoesNotExist()
        {
            // Given:
            var gameId = new GameId("dummy");
            var playerId = new PlayerId("dummy");

            _provider
                .Setup(p => p.GetGameByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game)null);

            // When:
            var testCode = new Func<Task>(() => _sut.GetGameViewForPlayerAsync(gameId, playerId, CancellationToken.None));

            // Then:
            await Assert.ThrowsAsync<GameNotFoundException>(testCode);
        }

        [Theory]
        [InlineData("player 1")]
        [InlineData("42")]
        [InlineData("true")]
        public async Task ReturnsGameView(string player)
        {
            // Given:
            var gameId = new GameId(42);
            var playerId = new PlayerId(player);
            var game = new Game(
                new GameInit(
                    new PlayerId(player),
                    new PlayerId("player 2"),
                    new PlayerId(player),
                    new Seed(42)));

            _provider
                .Setup(p => p.GetGameByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(game);

            // When:
            var actual = await _sut.GetGameViewForPlayerAsync(gameId, playerId, CancellationToken.None);

            // Then:
            Assert.NotNull(actual);

            Assert.Empty(actual.Chips);
            Assert.Equal(playerId, actual.CurrentPlayerId);
            Assert.Equal(7, actual.Hand.Count);
            Assert.Equal(90, actual.NumberOfCardsInDeck);
            Assert.Equal(2, actual.Players.Count);
            Assert.Equal(Team.Red, actual.Team);
            Assert.Null(actual.Winner);

            Assert.Equal(playerId, actual.Players[0].Id);
            Assert.Equal(7, actual.Players[0].NumberOfCards);
            Assert.Equal(Team.Red, actual.Players[0].Team);

            Assert.Equal(new PlayerId("player 2"), actual.Players[1].Id);
            Assert.Equal(7, actual.Players[1].NumberOfCards);
            Assert.Equal(Team.Green, actual.Players[1].Team);
        }
    }
}
