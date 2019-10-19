using Sequence.PlayCard;
using System.Collections.Immutable;
using Xunit;

namespace Sequence.Test.PlayCard
{
    public sealed class Game_ExchangeDeadCard
    {
        private static readonly Player _player1 = new Player(
            new PlayerId(1),
            new PlayerHandle("player 1"));

        private static readonly Player _player2 = new Player(
            new PlayerId(2),
            new PlayerHandle("player 2"));

        private static readonly PlayerHandle _playerDummy = new PlayerHandle("dummy");
        private static readonly Card _cardDummy = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);
        private static readonly Coord _expectedCoord = new Coord(-1, -1);

        private static readonly GameState _sut = new GameState(
            new GameInit(
                ImmutableList.Create(
                    _player1,
                    _player2),
                _player1.Id,
                new Seed(42),
                BoardType.OneEyedJack,
                2));

        [Fact]
        public void ThrowsIfPlayerIsNotInGame()
        {
            var ex = Assert.Throws<ExchangeDeadCardFailedException>(
                () => Game.ExchangeDeadCard(_sut, _playerDummy, _cardDummy)
            );

            Assert.Equal(ExchangeDeadCardError.PlayerIsNotInGame, ex.Error);
        }

        [Fact]
        public void ThrowsIfPlayerIsCurrentPlayer()
        {
            var ex = Assert.Throws<ExchangeDeadCardFailedException>(
                () => Game.ExchangeDeadCard(_sut, _player2.Handle, _cardDummy)
            );

            Assert.Equal(ExchangeDeadCardError.PlayerIsNotCurrentPlayer, ex.Error);
        }

        [Fact]
        public void ThrowsIfPlayerDoesNotHaveCard()
        {
            var card = new Card(DeckNo.Two, Suit.Spades, Rank.Ace);

            var ex = Assert.Throws<ExchangeDeadCardFailedException>(
                () => Game.ExchangeDeadCard(_sut, _player1.Handle, card)
            );

            Assert.Equal(ExchangeDeadCardError.PlayerDoesNotHaveCard, ex.Error);
        }

        [Fact]
        public void ThrowsIfCardIsNotDead()
        {
            var ex = Assert.Throws<ExchangeDeadCardFailedException>(
                () => Game.ExchangeDeadCard(_sut, _player1.Handle, _cardDummy)
            );

            Assert.Equal(ExchangeDeadCardError.CardIsNotDead, ex.Error);
        }

        [Fact]
        public void ThrowsIfPlayerHasAlreadyExchangedDeadCard()
        {
            var sut = _sut
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
                    winner: null))
                .Apply(new GameEvent(
                    byPlayerId: _player1.Id,
                    cardDrawn: new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                    cardUsed: new Card(DeckNo.One, Suit.Diamonds, Rank.Queen),
                    chip: Team.Red,
                    coord: _expectedCoord,
                    index: 1,
                    nextPlayerId: _player1.Id,
                    sequences: ImmutableArray<Seq>.Empty,
                    winner: null));

            var ex = Assert.Throws<ExchangeDeadCardFailedException>(
                () => Game.ExchangeDeadCard(sut, _player1.Handle, _cardDummy)
            );

            Assert.Equal(ExchangeDeadCardError.PlayerHasAlreadyExchangedDeadCard, ex.Error);
        }

        [Fact]
        public void HappyPath()
        {
            // Given:
            var sut = _sut
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

            var expected = new GameEvent(
                byPlayerId: _player1.Id,
                cardDrawn: new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                cardUsed: _cardDummy,
                chip: null,
                coord: _expectedCoord,
                index: 3,
                nextPlayerId: _player1.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null);

            // When:
            var actual = Game.ExchangeDeadCard(sut, _player1.Handle, _cardDummy);

            // Then:
            Assert.Equal(expected, actual, new GameEventEqualityComparer());
        }
    }
}
