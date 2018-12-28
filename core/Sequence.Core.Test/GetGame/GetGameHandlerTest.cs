using Moq;
using Sequence.Core.Test;
using System;
using System.Collections.Immutable;
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

        private readonly GameId _gameIdDummy = GameIdGenerator.Generate();
        private readonly PlayerId _playerIdDummy = new PlayerId("dummy");

        public GetGameHandlerTest()
        {
            _sut = new GetGameHandler(_provider.Object);
        }

        [Fact]
        public async Task ThrowsWhenArgsAreNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.GetGameViewForPlayerAsync(null, _playerIdDummy, CancellationToken.None)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "playerId",
                testCode: () => _sut.GetGameViewForPlayerAsync(_gameIdDummy, null, CancellationToken.None)
            );
        }

        [Fact]
        public async Task ThrowsWhenCanceled()
        {
            var cancellationToken = new CancellationToken(canceled: true);

            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: () => _sut.GetGameViewForPlayerAsync(_gameIdDummy, _playerIdDummy, cancellationToken)
            );
        }

        [Fact]
        public async Task ThrowsIfGameDoesNotExist()
        {
            // Given:
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameIdDummy, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game)null);

            // When:
            var testCode = new Func<Task>(() => _sut.GetGameViewForPlayerAsync(_gameIdDummy, _playerIdDummy, CancellationToken.None));

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
            var playerId = new PlayerId(player);
            var game = new Game(
                new GameInit(
                    ImmutableList.Create(
                        new PlayerId(player),
                        new PlayerId("player 2")),
                    new PlayerId(player),
                    new Seed(42)));

            _provider
                .Setup(p => p.GetGameByIdAsync(_gameIdDummy, It.IsAny<CancellationToken>()))
                .ReturnsAsync(game);

            // When:
            var actual = await _sut.GetGameViewForPlayerAsync(_gameIdDummy, playerId, CancellationToken.None);

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
