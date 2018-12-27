using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace Sequence.Core.Test
{
    public sealed class GameTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            var init = new GameInit(
                ImmutableList.Create(
                    new PlayerId("player 1"),
                    new PlayerId("player 2")),
                new PlayerId("oewfwoeoi"),
                new Seed(42)
            );

            var gameEvents = new GameEvent[0];

            Assert.Throws<ArgumentNullException>(
                paramName: "init",
                testCode: () => new Game(init: null, gameEvents)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "gameEvents",
                testCode: () => new Game(init, gameEvents: null)
            );
        }

        private readonly PlayerId _player1 = new PlayerId("player 1");
        private readonly PlayerId _player2 = new PlayerId("player 2");
        private readonly Game _sut;

        private readonly PlayerId _playerIdDummy = new PlayerId("dummy");
        private readonly Card _cardDummy = new Card(DeckNo.One, Suit.Spades, Rank.Ace);
        private readonly Card _oneEyedJack = new Card(DeckNo.One, Suit.Hearts, Rank.Jack);
        private readonly Coord _coordDummy = new Coord(4, 2);


        public GameTest()
        {
            _sut = new Game(
                new GameInit(
                    ImmutableList.Create(
                        _player1,
                        _player2),
                    _player1,
                    new Seed(42)));
        }

        [Fact]
        public void PlayCard_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "playerId",
                testCode: () => _sut.PlayCard(playerId: null, _cardDummy, _coordDummy)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "card",
                testCode: () => _sut.PlayCard(_playerIdDummy, card: null, _coordDummy)
            );
        }

        [Fact]
        public void ThrowsIfPlayerIsNotInGame()
        {
            var ex = Assert.Throws<PlayCardFailedException>(
                () => _sut.PlayCard(_playerIdDummy, _cardDummy, _coordDummy)
            );

            Assert.Equal(PlayCardError.PlayerIsNotInGame, ex.Error);
        }

        [Fact]
        public void ThrowsIfPlayerIsCurrentPlayer()
        {
            var ex = Assert.Throws<PlayCardFailedException>(
                () => _sut.PlayCard(_player2, _cardDummy, _coordDummy)
            );

            Assert.Equal(PlayCardError.PlayerIsNotCurrentPlayer, ex.Error);
        }

        [Fact]
        public void ThrowsIfCoordIsOccupied()
        {
            _sut.Apply(new GameEvent
            {
                ByPlayerId = _player2,
                CardDrawn = null,
                CardUsed = _cardDummy,
                Chip = Team.Green,
                Coord = _coordDummy,
                Index = 0,
                NextPlayerId = _player1,
            });

            var card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);

            var ex = Assert.Throws<PlayCardFailedException>(
                () => _sut.PlayCard(_player1, card, _coordDummy)
            );

            Assert.Equal(PlayCardError.CoordIsOccupied, ex.Error);
        }

        [Fact]
        public void ThrowsIfPlayerDoesNotHaveCard()
        {
            var ex = Assert.Throws<PlayCardFailedException>(
                () => _sut.PlayCard(_player1, _cardDummy, _coordDummy)
            );

            Assert.Equal(PlayCardError.PlayerDoesNotHaveCard, ex.Error);
        }

        [Fact]
        public void ThrowsIfCardDoesNotMatchCoord()
        {
            var card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);

            var ex = Assert.Throws<PlayCardFailedException>(
                () => _sut.PlayCard(_player1, card, _coordDummy)
            );

            Assert.Equal(PlayCardError.CardDoesNotMatchCoord, ex.Error);
        }

        [Fact]
        public void HappyPath()
        {
            // Given:
            var playerId = _player1;
            var card = new Card(DeckNo.Two, Suit.Spades, Rank.Ten);
            var coord = new Coord(1, 9);

            var expected = new GameEvent
            {
                ByPlayerId = playerId,
                CardDrawn = new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                CardUsed = card,
                Chip = Team.Red,
                Coord = coord,
                Index = 1,
                NextPlayerId = _player2,
            };

            // When:
            var actual = _sut.PlayCard(playerId, card, coord);

            // Then:
            Assert.Equal(expected, actual, new GameEventEqualityComparer());
        }

        [Fact]
        public void CannotPlayOneEyedJackIfCoordIsEmpty()
        {
            var playerId = _player1;
            var card = _oneEyedJack;
            var coord = _coordDummy;

            // Add a one-eyed jack to Player1 to use for this test.
            _sut.Apply(new GameEvent
            {
                ByPlayerId = _player1,
                CardDrawn = card,
                CardUsed = _cardDummy,
                Chip = Team.Red,
                Coord = new Coord(9, 9),
                Index = 0,
                NextPlayerId = _player1,
            });

            var ex = Assert.Throws<PlayCardFailedException>(
                () => _sut.PlayCard(playerId, card, coord)
            );

            Assert.Equal(PlayCardError.CoordIsEmpty, ex.Error);
        }

        [Fact]
        public void CannotPlayOneEyedJackIfCoordBelongsToPlayersOwnTeam()
        {
            var playerId = _player1;
            var card = _oneEyedJack;
            var coord = _coordDummy;

            // Add a one-eyed jack to Player1 to use for this test.
            _sut.Apply(new GameEvent
            {
                ByPlayerId = _player1,
                CardDrawn = card,
                CardUsed = _cardDummy,
                Chip = Team.Red,
                Coord = coord,
                Index = 0,
                NextPlayerId = _player1,
            });

            var ex = Assert.Throws<PlayCardFailedException>(
                () => _sut.PlayCard(playerId, card, coord)
            );

            Assert.Equal(PlayCardError.ChipBelongsToPlayerTeam, ex.Error);
        }

        [Fact]
        public void CanPlayOneEyedJack()
        {
            // Given:
            var playerId = _player1;
            var oneEyedJack = new Card(DeckNo.One, Suit.Hearts, Rank.Jack);
            var coord = _coordDummy;

            // Add a one-eyed jack to Player1 to use for this test.
            _sut.Apply(new GameEvent
            {
                ByPlayerId = _player1,
                CardDrawn = oneEyedJack,
                CardUsed = _cardDummy,
                Chip = Team.Red,
                Coord = new Coord(9, 9),
                Index = 0,
                NextPlayerId = _player2,
            });

            // Add a Team Green chip to some coordinate that Team Red can remove.
            _sut.Apply(new GameEvent
            {
                ByPlayerId = _player2,
                CardDrawn = null,
                CardUsed = _cardDummy,
                Chip = Team.Green,
                Coord = _coordDummy,
                Index = 1,
                NextPlayerId = _player1,
            });

            // When:
            var actual = _sut.PlayCard(playerId, oneEyedJack, coord);

            // Then:
            var expected = new GameEvent
            {
                ByPlayerId = playerId,
                CardDrawn = new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                CardUsed = oneEyedJack,
                Chip = null,
                Coord = coord,
                Index = 2,
                NextPlayerId = _player2,
            };

            Assert.Equal(expected, actual, new GameEventEqualityComparer());
        }

        private sealed class GameEventEqualityComparer : EqualityComparer<GameEvent>
        {
            public override bool Equals(GameEvent x, GameEvent y)
            {
                return x.ByPlayerId.Equals(y.ByPlayerId)
                    && x.CardDrawn.Equals(y.CardDrawn)
                    && x.CardUsed.Equals(y.CardUsed)
                    && x.Chip.Equals(y.Chip)
                    && x.Coord.Equals(y.Coord)
                    && x.Index.Equals(y.Index)
                    && x.NextPlayerId.Equals(y.NextPlayerId);
            }

            public override int GetHashCode(GameEvent obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
