using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace Sequence.Core
{
    public sealed class Game
    {
        private readonly Board _board;
        private readonly Deck _deck;
        private readonly ImmutableArray<PlayerId> _idxByPlayerId;
        private readonly ImmutableArray<Team> _teamByIdx;

        private ImmutableArray<IImmutableList<Card>> _handByIdx;
        private ImmutableStack<Card> _discards;
        private PlayerId _currentPlayerId;
        private int _index;
        private Winner _winner;

        public Game(GameInit init, params GameEvent[] gameEvents)
        {
            if (init == null)
            {
                throw new ArgumentNullException(nameof(init));
            }

            if (gameEvents == null)
            {
                throw new ArgumentNullException(nameof(gameEvents));
            }

            _board = new Board();
            _currentPlayerId = init.Player1;
            _deck = new Deck(init.Seed);
            _discards = ImmutableStack<Card>.Empty;
            _idxByPlayerId = ImmutableArray.Create(init.Player1, init.Player2);
            _handByIdx = ImmutableArray.Create(_deck.DealHand(), _deck.DealHand());
            _teamByIdx = ImmutableArray.Create(Team.Red, Team.Green);
            _winner = null;

            foreach (var gameEvent in gameEvents)
            {
                Apply(gameEvent);
            }
        }

        public void Apply(GameEvent gameEvent)
        {
            var cardDrawn = gameEvent.CardDrawn;
            var cardUsed = gameEvent.CardUsed;
            var playerIdx = _idxByPlayerId.IndexOf(gameEvent.ByPlayerId);
            var hand = _handByIdx[playerIdx].Remove(cardUsed);

            if (cardDrawn != null)
            {
                hand = hand.Add(cardDrawn);
            }

            _deck.Remove(cardDrawn);
            _discards = _discards.Push(cardUsed);
            _handByIdx = _handByIdx.SetItem(playerIdx, hand);
            _board.Add(gameEvent.Coord, gameEvent.Chip);

            if (_board.Sequence == null)
            {
                _currentPlayerId = gameEvent.NextPlayerId;
                _winner = null;
            }
            else
            {
                _currentPlayerId = null;
                _winner = new Winner(_board.Sequence.Team);
            }

            _index = gameEvent.Index;
        }

        public GameEvent PlayCard(PlayerId playerId, Card card, Coord coord)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            var playerIdx = _idxByPlayerId.IndexOf(playerId);

            if (playerIdx == -1)
            {
                throw new PlayCardFailedException(PlayCardError.PlayerIsNotInGame);
            }

            if (!playerId.Equals(_currentPlayerId))
            {
                throw new PlayCardFailedException(PlayCardError.PlayerIsNotCurrentPlayer);
            }

            if (!_handByIdx[playerIdx].Contains(card))
            {
                throw new PlayCardFailedException(PlayCardError.PlayerDoesNotHaveCard);
            }

            if (card.IsOneEyedJack())
            {
                if (!_board.IsOccupied(coord))
                {
                    throw new PlayCardFailedException(PlayCardError.CoordIsEmpty);
                }

                if (_board.Chips.TryGetValue(coord, out var chip) && chip == _teamByIdx[playerIdx])
                {
                    throw new PlayCardFailedException(PlayCardError.ChipBelongsToPlayerTeam);
                }

                // TODO: Check chip is not part of sequence when multiple sequences are supported in future.

                return new GameEvent
                {
                    ByPlayerId = playerId,
                    CardDrawn = _deck.Top,
                    CardUsed = card,
                    Chip = null,
                    Coord = coord,
                    Index = _index + 1,
                    NextPlayerId = _idxByPlayerId[(playerIdx + 1) % _idxByPlayerId.Length],
                };
            }
            else
            {
                if (_board.IsOccupied(coord))
                {
                    throw new PlayCardFailedException(PlayCardError.CoordIsOccupied);
                }

                if (!Board.Matches(coord, card))
                {
                    throw new PlayCardFailedException(PlayCardError.CardDoesNotMatchCoord);
                }

                return new GameEvent
                {
                    ByPlayerId = playerId,
                    CardDrawn = _deck.Top,
                    CardUsed = card,
                    Chip = _teamByIdx[playerIdx],
                    Coord = coord,
                    Index = _index + 1,
                    NextPlayerId = _idxByPlayerId[(playerIdx + 1) % _idxByPlayerId.Length],
                };
            }
        }

        public GameView GetViewForPlayer(PlayerId playerId)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            var view = new GameView
            {
                Board = Board.TheBoard,
                Chips = _board.Chips.Select(c => new ChipView
                {
                    Coord = c.Key,
                    IsLocked = _board.Sequence?.Coords.Contains(c.Key) == true,
                    Team = c.Value,
                }).ToImmutableArray(),
                CurrentPlayerId = _currentPlayerId,
                Discards = _discards.ToImmutableArray(),
                NumberOfCardsInDeck = _deck.Count,
                Players = _idxByPlayerId.Select((p, i) => new PlayerView
                {
                    Id = p,
                    NumberOfCards = _handByIdx[i].Count,
                    Team = _teamByIdx[i],
                }).ToImmutableArray(),
                Winner = _winner,
            };

            var idx = _idxByPlayerId.IndexOf(playerId);

            if (idx >= 0)
            {
                view.Hand = _handByIdx[idx];
                view.Team = _teamByIdx[idx];
            }

            return view;
        }
    }

    public sealed class PlayCardFailedException : Exception
    {
        public PlayCardFailedException(PlayCardError error)
        {
            Error = error;
        }

        public PlayCardError Error { get; }
    }

    public enum PlayCardError
    {
        PlayerIsNotInGame,
        PlayerIsNotCurrentPlayer,
        CoordIsOccupied,
        PlayerDoesNotHaveCard,
        CardDoesNotMatchCoord,
        CoordIsEmpty,
        ChipBelongsToPlayerTeam,
    }

    public enum Team
    {
        Red, Green, Blue,
    }

    public sealed class Winner
    {
        public Winner(Team team)
        {
            Team = team;
        }

        public Team Team { get; }
    }

    public sealed class PlayerView
    {
        public PlayerId Id { get; internal set; }
        public int NumberOfCards { get; internal set; }
        public Team Team { get; internal set; }
    }

    public struct Coord : IEquatable<Coord>
    {
        public Coord(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }
        public int Row { get; }

        public bool Equals(Coord other)
        {
            return Column.Equals(other.Column) && Row.Equals(other.Row);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj.GetType() != typeof(Coord))
            {
                return false;
            }

            return Equals((Coord)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = -246786518;
                hashCode ^= Column.GetHashCode();
                hashCode ^= Row.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"(Column, Row): ({Column}, {Row})";
        }
    }

    public sealed class ChipView
    {
        public Coord Coord { get; internal set; }
        public bool IsLocked { get; internal set; }
        public Team Team { get; internal set; }
    }

    public sealed class GameView
    {
        public ImmutableArray<ImmutableArray<(Suit, Rank)?>> Board { get; internal set; }
        public IImmutableList<ChipView> Chips { get; internal set; }
        public PlayerId CurrentPlayerId { get; internal set; }
        public IImmutableList<Card> Discards { get; internal set; }
        public IImmutableList<Card> Hand { get; internal set; }
        public int NumberOfCardsInDeck { get; internal set; }
        public IImmutableList<PlayerView> Players { get; internal set; }
        public Team Team { get; internal set; }
        public Winner Winner { get; internal set; }
    }
}
