using Sequence.PlayCard;
using System.Collections.Immutable;
using Xunit;

namespace Sequence.Test.PlayCard
{
    public sealed partial class GameTest
    {
        private readonly Player _player1 = new Player(
            new PlayerId(1),
            new PlayerHandle("player 1"));

        private readonly Player _player2 = new Player(
            new PlayerId(2),
            new PlayerHandle("player 2"));

        private readonly GameState _sut;

        private readonly PlayerHandle _playerDummy = new PlayerHandle("dummy");
        private readonly Card _cardDummy = new Card(DeckNo.One, Suit.Spades, Rank.Ace);
        private readonly Card _oneEyedJack = new Card(DeckNo.One, Suit.Hearts, Rank.Jack);
        private readonly Coord _coordDummy = new Coord(4, 2);


        public GameTest()
        {
            _sut = new GameState(
                new GameInit(
                    ImmutableList.Create(
                        _player1,
                        _player2),
                    _player1.Id,
                    new Seed(42),
                    BoardType.OneEyedJack,
                    2));
        }

        [Fact]
        public void ThrowsIfPlayerIsNotInGame()
        {
            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(_sut, _playerDummy, _cardDummy, _coordDummy)
            );

            Assert.Equal(PlayCardError.PlayerIsNotInGame, ex.Error);
        }

        [Fact]
        public void ThrowsIfPlayerIsCurrentPlayer()
        {
            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(_sut, _player2.Handle, _cardDummy, _coordDummy)
            );

            Assert.Equal(PlayCardError.PlayerIsNotCurrentPlayer, ex.Error);
        }

        [Fact]
        public void ThrowsIfCoordIsOccupied()
        {
            var sut = _sut;

            sut = GameState.Apply(sut, new GameEvent(
                byPlayerId: _player2.Id,
                cardDrawn: null,
                cardUsed: _cardDummy,
                chip: Team.Green,
                coord: _coordDummy,
                index: 1,
                nextPlayerId: _player1.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            var card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);

            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(sut, _player1.Handle, card, _coordDummy)
            );

            Assert.Equal(PlayCardError.CoordIsOccupied, ex.Error);
        }

        [Fact]
        public void ThrowsIfPlayerDoesNotHaveCard()
        {
            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(_sut, _player1.Handle, _cardDummy, _coordDummy)
            );

            Assert.Equal(PlayCardError.PlayerDoesNotHaveCard, ex.Error);
        }

        [Fact]
        public void ThrowsIfCardDoesNotMatchCoord()
        {
            var card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);

            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(_sut, _player1.Handle, card, _coordDummy)
            );

            Assert.Equal(PlayCardError.CardDoesNotMatchCoord, ex.Error);
        }

        [Fact]
        public void HappyPath()
        {
            // Given:
            var card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);
            var coord = new Coord(1, 9);

            var expected = new GameEvent(
                byPlayerId: _player1.Id,
                cardDrawn: new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                cardUsed: card,
                chip: Team.Red,
                coord: coord,
                index: 1,
                nextPlayerId: _player2.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null);

            // When:
            var actual = Game.PlayCard(_sut, _player1.Handle, card, coord);

            // Then:
            Assert.Equal(expected, actual, new GameEventEqualityComparer());
        }

        [Fact]
        public void CannotPlayOneEyedJackIfCoordIsEmpty()
        {
            var card = _oneEyedJack;
            var coord = _coordDummy;
            var sut = _sut;

            // Add a one-eyed jack to Player1 to use for this test.
            sut = GameState.Apply(sut, new GameEvent(
                byPlayerId: _player1.Id,
                cardDrawn: card,
                cardUsed: _cardDummy,
                chip: Team.Red,
                coord: new Coord(9, 9),
                index: 1,
                nextPlayerId: _player1.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(sut, _player1.Handle, card, coord)
            );

            Assert.Equal(PlayCardError.CoordIsEmpty, ex.Error);
        }

        [Fact]
        public void CannotPlayOneEyedJackIfCoordBelongsToPlayersOwnTeam()
        {
            var card = _oneEyedJack;
            var coord = _coordDummy;
            var sut = _sut;

            // Add a one-eyed jack to Player1 to use for this test.
            sut = GameState.Apply(sut, new GameEvent(
                byPlayerId: _player1.Id,
                cardDrawn: card,
                cardUsed: _cardDummy,
                chip: Team.Red,
                coord: coord,
                index: 1,
                nextPlayerId: _player1.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(sut, _player1.Handle, card, coord)
            );

            Assert.Equal(PlayCardError.ChipBelongsToPlayerTeam, ex.Error);
        }

        [Fact]
        public void CannotPlayOneEyedJackIfCoordIsPartOfSequence()
        {
            var card = _oneEyedJack;
            var coord = _coordDummy;
            var sut = _sut;

            // Add Player1 sequence.
            sut = GameState.Apply(sut, new GameEvent(
                byPlayerId: _player1.Id,
                cardDrawn: card,
                cardUsed: _cardDummy,
                chip: Team.Red,
                coord: coord,
                index: 1,
                nextPlayerId: _player2.Id,
                sequences: ImmutableArray.Create(
                    new Seq(Team.Red, ImmutableArray.Create(coord, coord, coord, coord, coord))),
                winner: null));

            // Add a one-eyed jack to Player2 to use for this test.
            sut = GameState.Apply(sut, new GameEvent(
                byPlayerId: _player2.Id,
                cardDrawn: card,
                cardUsed: _cardDummy,
                chip: Team.Green,
                coord: new Coord(2, 4),
                index: 2,
                nextPlayerId: _player2.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            var ex = Assert.Throws<PlayCardFailedException>(
                () => Game.PlayCard(sut, _player2.Handle, card, coord)
            );

            Assert.Equal(PlayCardError.ChipIsPartOfSequence, ex.Error);
        }

        [Fact]
        public void CanPlayOneEyedJack()
        {
            // Given:
            var oneEyedJack = new Card(DeckNo.One, Suit.Hearts, Rank.Jack);
            var coord = _coordDummy;
            var sut = _sut;

            // Add a one-eyed jack to Player1 to use for this test.
            sut = GameState.Apply(sut, new GameEvent(
                byPlayerId: _player1.Id,
                cardDrawn: oneEyedJack,
                cardUsed: _cardDummy,
                chip: Team.Red,
                coord: new Coord(9, 9),
                index: 1,
                nextPlayerId: _player2.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            // Add a Team Green chip to some coordinate that Team Red can remove.
            sut = GameState.Apply(sut, new GameEvent(
                byPlayerId: _player2.Id,
                cardDrawn: null,
                cardUsed: _cardDummy,
                chip: Team.Green,
                coord: _coordDummy,
                index: 2,
                nextPlayerId: _player1.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            // When:
            var actual = Game.PlayCard(sut, _player1.Handle, oneEyedJack, coord);

            // Then:
            var expected = new GameEvent(
                byPlayerId: _player1.Id,
                cardDrawn: new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                cardUsed: oneEyedJack,
                chip: null,
                coord: coord,
                index: 3,
                nextPlayerId: _player2.Id,
                sequences: ImmutableArray<Seq>.Empty,
                winner: null);

            Assert.Equal(expected, actual, new GameEventEqualityComparer());
        }
    }
}
