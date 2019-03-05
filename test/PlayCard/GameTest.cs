using Sequence.PlayCard;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Sequence.Test.PlayCard
{
    public sealed partial class GameTest
    {
        private readonly Player _player1 = new Player(
            new PlayerId(1),
            new PlayerHandle("player 1")
        );

        private readonly Player _player2 = new Player(
            new PlayerId(2),
            new PlayerHandle("player 2")
        );

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
        public void PlayCard_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "state",
                testCode: () => Game.PlayCard(state: null, _playerDummy, _cardDummy, _coordDummy)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "player",
                testCode: () => Game.PlayCard(_sut, player: (PlayerHandle)null, _cardDummy, _coordDummy)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "player",
                testCode: () => Game.PlayCard(_sut, player: (PlayerId)null, _cardDummy, _coordDummy)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "card",
                testCode: () => Game.PlayCard(_sut, _playerDummy, card: null, _coordDummy)
            );
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

            sut = GameState.Apply(sut, new GameEvent
            {
                ByPlayerId = _player2.Id,
                CardDrawn = null,
                CardUsed = _cardDummy,
                Chip = Team.Green,
                Coord = _coordDummy,
                Index = 1,
                NextPlayerId = _player1.Id,
            });

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

            var expected = new GameEvent
            {
                ByPlayerId = _player1.Id,
                CardDrawn = new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                CardUsed = card,
                Chip = Team.Red,
                Coord = coord,
                Index = 1,
                NextPlayerId = _player2.Id,
            };

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
            sut = GameState.Apply(sut, new GameEvent
            {
                ByPlayerId = _player1.Id,
                CardDrawn = card,
                CardUsed = _cardDummy,
                Chip = Team.Red,
                Coord = new Coord(9, 9),
                Index = 1,
                NextPlayerId = _player1.Id,
            });

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
            sut = GameState.Apply(sut, new GameEvent
            {
                ByPlayerId = _player1.Id,
                CardDrawn = card,
                CardUsed = _cardDummy,
                Chip = Team.Red,
                Coord = coord,
                Index = 1,
                NextPlayerId = _player1.Id,
            });

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
            sut = GameState.Apply(sut, new GameEvent
            {
                ByPlayerId = _player1.Id,
                CardDrawn = card,
                CardUsed = _cardDummy,
                Chip = Team.Red,
                Coord = coord,
                Index = 1,
                NextPlayerId = _player2.Id,
                Sequences = new[]
                {
                    new Seq(Team.Red, ImmutableArray.Create(coord, coord, coord, coord, coord))
                },
            });

            // Add a one-eyed jack to Player2 to use for this test.
            sut = GameState.Apply(sut, new GameEvent
            {
                ByPlayerId = _player2.Id,
                CardDrawn = card,
                CardUsed = _cardDummy,
                Chip = Team.Green,
                Coord = new Coord(2, 4),
                Index = 2,
                NextPlayerId = _player2.Id,
            });

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
            sut = GameState.Apply(sut, new GameEvent
            {
                ByPlayerId = _player1.Id,
                CardDrawn = oneEyedJack,
                CardUsed = _cardDummy,
                Chip = Team.Red,
                Coord = new Coord(9, 9),
                Index = 1,
                NextPlayerId = _player2.Id,
            });

            // Add a Team Green chip to some coordinate that Team Red can remove.
            sut = GameState.Apply(sut, new GameEvent
            {
                ByPlayerId = _player2.Id,
                CardDrawn = null,
                CardUsed = _cardDummy,
                Chip = Team.Green,
                Coord = _coordDummy,
                Index = 2,
                NextPlayerId = _player1.Id,
            });

            // When:
            var actual = Game.PlayCard(sut, _player1.Handle, oneEyedJack, coord);

            // Then:
            var expected = new GameEvent
            {
                ByPlayerId = _player1.Id,
                CardDrawn = new Card(DeckNo.Two, Suit.Clubs, Rank.Ten),
                CardUsed = oneEyedJack,
                Chip = null,
                Coord = coord,
                Index = 3,
                NextPlayerId = _player2.Id,
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
                    && (x.NextPlayerId?.Equals(y.NextPlayerId) ?? true)
                    && x.Sequences.SequenceEqual(y.Sequences);
            }

            public override int GetHashCode(GameEvent obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
