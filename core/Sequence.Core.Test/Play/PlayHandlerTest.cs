using Moq;
using Sequence.Core.Play;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Core.Test
{
    public sealed class PlayHandlerTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            var provider = Mock.Of<IGameProvider>();
            var store = Mock.Of<IGameEventStore>();
            var notifier = Mock.Of<IGameUpdatedNotifier>();

            Assert.Throws<ArgumentNullException>(
                paramName: "provider",
                () => new PlayHandler(provider: null, store, notifier)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "store",
                () => new PlayHandler(provider, store: null, notifier)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "notifier",
                () => new PlayHandler(provider, store, notifier: null)
            );
        }

        private readonly Mock<IGameProvider> _provider = new Mock<IGameProvider>();
        private readonly Mock<IGameEventStore> _store = new Mock<IGameEventStore>();
        private readonly Mock<IGameUpdatedNotifier> _notifier = new Mock<IGameUpdatedNotifier>();
        private readonly PlayHandler _sut;

        private readonly GameId _gameId = GameIdGenerator.Generate();
        private readonly PlayerHandle _player = new PlayerHandle("dummy 1");
        private readonly Card _card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);
        private readonly Coord _coord = new Coord(1, 9);

        private readonly Game _game;

        public PlayHandlerTest()
        {
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game)null);

            _store
                .Setup(s => s.AddEventAsync(_gameId, It.IsAny<GameEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _notifier
                .Setup(n => n.SendAsync(_gameId, It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            _sut = new PlayHandler(_provider.Object, _store.Object, _notifier.Object);

            _game = new Game(
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
                    seed: new Seed(42)
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
                testCode: () => _sut.PlayCardAsync(_gameId, player: null, _card, _coord, CancellationToken.None)
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
        public async Task SavesEvent()
        {
            // Given:
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_game)
                .Verifiable();

            // When:
            await _sut.PlayCardAsync(_gameId, _player, _card, _coord, CancellationToken.None);

            // Then:
            _store.VerifyAll();
        }

        [Fact]
        public async Task PublishesEvent()
        {
            // Given:
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_game);

            _store
                .Setup(s => s.AddEventAsync(_gameId, It.IsAny<GameEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // When:
            await _sut.PlayCardAsync(_gameId, _player, _card, _coord, CancellationToken.None);

            // Then:
            _store.VerifyAll();
        }
    }
}
