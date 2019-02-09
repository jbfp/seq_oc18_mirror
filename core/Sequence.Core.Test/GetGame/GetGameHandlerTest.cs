using Moq;
using Sequence.Core.GetGame;
using Sequence.Core.Test;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Core.Test.GetGame
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
        private readonly PlayerHandle _playerDummy = new PlayerHandle("dummy");

        public GetGameHandlerTest()
        {
            _sut = new GetGameHandler(_provider.Object);
        }

        [Fact]
        public async Task ThrowsWhenArgsAreNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.GetGameViewForPlayerAsync(null, _playerDummy, CancellationToken.None)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "player",
                testCode: () => _sut.GetGameViewForPlayerAsync(_gameIdDummy, null, CancellationToken.None)
            );
        }

        [Fact]
        public async Task ThrowsWhenCanceled()
        {
            var cancellationToken = new CancellationToken(canceled: true);

            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: () => _sut.GetGameViewForPlayerAsync(_gameIdDummy, _playerDummy, cancellationToken)
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
            var testCode = new Func<Task>(() => _sut.GetGameViewForPlayerAsync(_gameIdDummy, _playerDummy, CancellationToken.None));

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
            var playerId = new PlayerId(1);
            var playerHandle = new PlayerHandle(player);
            var game = new Game(
                new GameInit(
                    players: ImmutableList.Create(
                        new Player(playerId, playerHandle),
                        new Player(new PlayerId(2), new PlayerHandle("Player 2"))),
                    firstPlayerId: playerId,
                    seed: new Seed(42),
                    boardType: BoardType.OneEyedJack,
                    numSequencesToWin: 2));

            _provider
                .Setup(p => p.GetGameByIdAsync(_gameIdDummy, It.IsAny<CancellationToken>()))
                .ReturnsAsync(game);

            // When:
            var actual = await _sut.GetGameViewForPlayerAsync(_gameIdDummy, playerHandle, CancellationToken.None);

            // Then:
            Assert.NotNull(actual);

            Assert.Empty(actual.Chips);
            Assert.Equal(playerId, actual.CurrentPlayerId);
            Assert.Equal(7, actual.Hand.Count);
            Assert.Equal(90, actual.NumberOfCardsInDeck);
            Assert.Equal(2, actual.NumberOfSequencesToWin);
            Assert.Equal(2, actual.Players.Count);
            Assert.Equal(BoardType.OneEyedJack, actual.Rules.BoardType);
            Assert.Equal(2, actual.Rules.WinCondition);
            Assert.Equal(Team.Red, actual.Team);
            Assert.Null(actual.Winner);

            Assert.Equal(playerId, actual.PlayerId);
            Assert.Equal(playerId, actual.Players[0].Id);
            Assert.Equal(playerHandle, actual.Players[0].Handle);
            Assert.Equal(7, actual.Players[0].NumberOfCards);
            Assert.Equal(Team.Red, actual.Players[0].Team);
            Assert.Equal(PlayerType.User, actual.Players[0].Type);

            Assert.Equal(new PlayerId(2), actual.Players[1].Id);
            Assert.Equal(new PlayerHandle("Player 2"), actual.Players[1].Handle);
            Assert.Equal(7, actual.Players[1].NumberOfCards);
            Assert.Equal(Team.Green, actual.Players[1].Team);
        }
    }
}
