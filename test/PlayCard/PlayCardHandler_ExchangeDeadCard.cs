using Moq;
using Sequence.PlayCard;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Test.PlayCard
{
    public sealed class PlayCardHandler_ExchangeDeadCard
    {
        private static readonly Player _player1 = new Player(
            new PlayerId(1),
            new PlayerHandle("player 1"));

        private static readonly Player _player2 = new Player(
            new PlayerId(2),
            new PlayerHandle("player 2"));

        private readonly Mock<IGameStateProvider> _provider = new Mock<IGameStateProvider>();
        private readonly Mock<IGameEventStore> _store = new Mock<IGameEventStore>();
        private readonly Mock<IRealTimeContext> _realTime = new Mock<IRealTimeContext>();

        private readonly PlayCardHandler _sut;

        private readonly GameId _gameId = GameIdGenerator.Generate();
        private readonly Card _deadCard = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);

        private readonly GameState _game;

        public PlayCardHandler_ExchangeDeadCard()
        {
            _provider
                .Setup(p => p.GetGameByIdAsync(_gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GameState?)null)
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
                     ImmutableList.Create(
                    _player1,
                    _player2),
                _player1.Id,
                new Seed(42),
                BoardType.OneEyedJack,
                2))
                .Apply(new GameEvent(
                    byPlayerId: _player1.Id,
                    cardDrawn: null,
                    cardUsed: new Card(DeckNo.One, Suit.Clubs, Rank.Jack),
                    chip: Team.Red,
                    coord: new Coord(1, 9),
                    index: 1,
                    nextPlayerId: _player1.Id,
                    sequences: ImmutableArray<Seq>.Empty,
                    winner: null))
                .Apply(new GameEvent(
                    byPlayerId: _player1.Id,
                    cardDrawn: null,
                    cardUsed: new Card(DeckNo.Two, Suit.Clubs, Rank.Jack),
                    chip: Team.Red,
                    coord: new Coord(8, 0),
                    index: 2,
                    nextPlayerId: _player1.Id,
                    sequences: ImmutableArray<Seq>.Empty,
                    winner: null));
        }

        [Fact]
        public async Task ThrowsWhenCanceled()
        {
            var cancellationToken = new CancellationToken(canceled: true);

            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: () => _sut.ExchangeDeadCardAsync(_gameId, _player1.Id, _deadCard, cancellationToken)
            );
        }

        [Fact]
        public async Task ThrowsIfGameDoesNotExist()
        {
            await Assert.ThrowsAsync<GameNotFoundException>(
                () => _sut.ExchangeDeadCardAsync(_gameId, _player1.Id, _deadCard, CancellationToken.None)
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
            await _sut.ExchangeDeadCardAsync(_gameId, _player1.Id, _deadCard, CancellationToken.None);

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
            await _sut.ExchangeDeadCardAsync(_gameId, _player1.Id, _deadCard, CancellationToken.None);

            // Then:
            _store.Verify();
        }
    }
}
