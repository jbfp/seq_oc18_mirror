using Moq;
using Sequence.PlayCard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Test.PlayCard
{
    public sealed class PlayCardHandlerTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            var provider = Mock.Of<IGameStateProvider>();
            var store = Mock.Of<IGameEventStore>();
            var realTime = Mock.Of<IRealTimeContext>();

            Assert.Throws<ArgumentNullException>(
                paramName: "provider",
                () => new PlayCardHandler(provider: null, store, realTime)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "store",
                () => new PlayCardHandler(provider, store: null, realTime)
            );

            Assert.Throws<ArgumentNullException>(
               paramName: "realTime",
               () => new PlayCardHandler(provider, store, realTime: null)
           );
        }

        private readonly Mock<IGameStateProvider> _provider = new Mock<IGameStateProvider>();
        private readonly Mock<IGameEventStore> _store = new Mock<IGameEventStore>();
        private readonly Mock<IRealTimeContext> _realTime = new Mock<IRealTimeContext>();

        private readonly PlayCardHandler _sut;

        private readonly GameId _gameId = GameIdGenerator.Generate();
        private readonly PlayerHandle _player = new PlayerHandle("dummy 1");
        private readonly Card _card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);
        private readonly Coord _coord = new Coord(1, 9);

        private readonly GameState _game;

        public PlayCardHandlerTest()
        {
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GameState)null)
                .Verifiable();

            _store
                .Setup(s => s.AddEventAsync(_gameId, It.IsAny<GameEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _realTime
                .Setup(r => r.SendGameUpdatesAsync(It.IsAny<PlayerId>(), It.IsAny<IEnumerable<GameUpdated>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut = new PlayCardHandler(_provider.Object, _store.Object, _realTime.Object);

            _game = new GameState(
                new GameInit(
                    players: ImmutableArray.Create(
                        new Player(
                            new PlayerId(1),
                            _player
                        ),
                        new Player(
                            new PlayerId(2),
                            new PlayerHandle("dummy 2")
                        )
                    ),
                    firstPlayerId: new PlayerId(1),
                    seed: new Seed(42),
                    boardType: BoardType.OneEyedJack,
                    numSequencesToWin: 2
                )
            );
        }

        [Fact]
        public async Task ThrowsWhenArgsAreNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.PlayCardAsync(gameId: null, _player, _card, _coord, CancellationToken.None)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "player",
                testCode: () => _sut.PlayCardAsync(_gameId, player: (PlayerHandle)null, _card, _coord, CancellationToken.None)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "player",
                testCode: () => _sut.PlayCardAsync(_gameId, player: (PlayerId)null, _card, _coord, CancellationToken.None)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "card",
                testCode: () => _sut.PlayCardAsync(_gameId, _player, card: null, _coord, CancellationToken.None)
            );
        }

        [Fact]
        public async Task ThrowsWhenCanceled()
        {
            var cancellationToken = new CancellationToken(canceled: true);

            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: () => _sut.PlayCardAsync(_gameId, _player, _card, _coord, cancellationToken)
            );
        }

        [Fact]
        public async Task ThrowsIfGameDoesNotExist()
        {
            await Assert.ThrowsAsync<GameNotFoundException>(
                () => _sut.PlayCardAsync(_gameId, _player, _card, _coord, CancellationToken.None)
            );
        }

        [Fact]
        public async Task GetsGameFromProvider()
        {
            // Given:
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_game)
                .Verifiable();

            // When:
            await _sut.PlayCardAsync(_gameId, _player, _card, _coord, CancellationToken.None);

            // Then:
            _provider.Verify();
        }

        [Fact]
        public async Task SavesEvent()
        {
            // Given:
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_game);

            // When:
            await _sut.PlayCardAsync(_gameId, _player, _card, _coord, CancellationToken.None);

            // Then:
            _store.Verify();
        }

        [Fact]
        public async Task UpdatesRealTimeComms()
        {
            // Given:
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_game);

            // When:
            await _sut.PlayCardAsync(_gameId, _player, _card, _coord, CancellationToken.None);
            await Task.Delay(1000); // Updating comms happens on threadpool.

            // Then:
            _realTime.Verify();
        }
    }
}
