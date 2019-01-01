using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Sequence.Core
{
    public sealed class Game
    {
        private readonly Board _board;
        private readonly Deck _deck;
        private readonly ImmutableArray<PlayerHandle> _playerHandleByIdx;
        private readonly ImmutableArray<PlayerId> _playerIdByIdx;
        private readonly ImmutableArray<PlayerType> _playerTypeByIdx;
        private readonly ImmutableArray<Team> _teamByIdx;

        private ImmutableArray<IImmutableList<Card>> _handByIdx;
        private ImmutableStack<Card> _discards;
        private PlayerId _currentPlayerId;
        private Seq _sequence;
        private Winner _winner;
        private int _index;

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
            _currentPlayerId = init.FirstPlayerId;
            _deck = new Deck(init.Seed, init.Players.Count);
            _discards = ImmutableStack<Card>.Empty;
            _handByIdx = _deck.DealHands().ToImmutableArray();
            _playerHandleByIdx = init.Players.Select(p => p.Handle).ToImmutableArray();
            _playerIdByIdx = init.Players.Select(p => p.Id).ToImmutableArray();
            _playerTypeByIdx = init.Players.Select(p => p.Type).ToImmutableArray();

            ImmutableArray<Team> GetTeams()
            {
                switch (init.Players.Count)
                {
                    case 2: return ImmutableArray.Create(Team.Red, Team.Green);
                    case 3: return ImmutableArray.Create(Team.Red, Team.Green, Team.Blue);
                    case 4: return ImmutableArray.Create(Team.Red, Team.Green, Team.Red, Team.Green);
                    case 6: return ImmutableArray.Create(Team.Red, Team.Green, Team.Blue, Team.Red, Team.Green, Team.Blue);
                    default: throw new NotSupportedException();
                }
            }

            _teamByIdx = GetTeams();

            foreach (var gameEvent in gameEvents)
            {
                Apply(gameEvent);
            }
        }

        public void Apply(GameEvent gameEvent)
        {
            if (_index == gameEvent.Index)
            {
                return;
            }

            if (_index > gameEvent.Index)
            {
                throw new InvalidOperationException($"Game event cannot be applied. Index is {_index}, game event index is {gameEvent.Index}.");
            }

            var cardDrawn = gameEvent.CardDrawn;
            var cardUsed = gameEvent.CardUsed;
            var playerIdx = _playerIdByIdx.IndexOf(gameEvent.ByPlayerId);
            var hand = _handByIdx[playerIdx].Remove(cardUsed);

            if (cardDrawn != null)
            {
                hand = hand.Add(cardDrawn);
            }

            _deck.Remove(cardDrawn);
            _discards = _discards.Push(cardUsed);
            _handByIdx = _handByIdx.SetItem(playerIdx, hand);
            _board.Add(gameEvent.Coord, gameEvent.Chip);
            _currentPlayerId = gameEvent.NextPlayerId;
            _sequence = gameEvent.Sequence;
            _index = gameEvent.Index;

            if (_sequence != null)
            {
                _winner = new Winner(_sequence.Team);
            }
            else
            {
                _winner = null;
            }
        }

        public GameEvent PlayCard(PlayerHandle player, Card card, Coord coord)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            var playerIdx = _playerHandleByIdx.IndexOf(player);

            if (playerIdx == -1)
            {
                throw new PlayCardFailedException(PlayCardError.PlayerIsNotInGame);
            }

            return PlayCard(_playerIdByIdx[playerIdx], card, coord);
        }

        internal GameEvent PlayCard(PlayerId playerId, Card card, Coord coord)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (!playerId.Equals(_currentPlayerId))
            {
                throw new PlayCardFailedException(PlayCardError.PlayerIsNotCurrentPlayer);
            }

            var playerIdx = _playerIdByIdx.IndexOf(playerId);

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
                    NextPlayerId = _playerIdByIdx[(playerIdx + 1) % _playerIdByIdx.Length],
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

                var team = _teamByIdx[playerIdx];
                var sequence = Board.GetSequence(_board.Chips.Add(coord, team), coord, team);
                var nextPlayerId = sequence == null ? _playerIdByIdx[(playerIdx + 1) % _playerIdByIdx.Length] : null;

                return new GameEvent
                {
                    ByPlayerId = playerId,
                    CardDrawn = _deck.Top,
                    CardUsed = card,
                    Chip = team,
                    Coord = coord,
                    Index = _index + 1,
                    NextPlayerId = nextPlayerId,
                    Sequence = sequence,
                };
            }
        }

        public GameView GetViewForPlayer(PlayerHandle player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var idx = _playerHandleByIdx.IndexOf(player);
            var playerId = idx >= 0 ? _playerIdByIdx[idx] : null;
            return GetViewForPlayer(playerId);
        }

        internal GameView GetViewForPlayer(PlayerId playerId)
        {
            var view = new GameView
            {
                Board = Board.TheBoard,
                Chips = _board.Chips.Select(c => new ChipView
                {
                    Coord = c.Key,
                    IsLocked = _sequence?.Coords.Contains(c.Key) == true,
                    Team = c.Value,
                }).ToImmutableArray(),
                CurrentPlayerId = _currentPlayerId,
                Discards = _discards.ToImmutableArray(),
                NumberOfCardsInDeck = _deck.Count,
                Players = _playerIdByIdx.Select((p, i) => new PlayerView
                {
                    Id = p,
                    Handle = _playerHandleByIdx[i],
                    NumberOfCards = _handByIdx[i].Count,
                    Team = _teamByIdx[i],
                    Type = _playerTypeByIdx[i],
                }).ToImmutableArray(),
                Winner = _winner,
            };

            var idx = _playerIdByIdx.IndexOf(playerId);

            if (idx >= 0)
            {
                view.Hand = _handByIdx[idx];
                view.PlayerId = _playerIdByIdx[idx];
                view.Team = _teamByIdx[idx];
            }

            return view;
        }
    }

    public sealed class PlayCardFailedException : Exception
    {
        public PlayCardFailedException(PlayCardError error) : base(error.ToString())
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
        public PlayerHandle Handle { get; internal set; }
        public int NumberOfCards { get; internal set; }
        public Team Team { get; internal set; }
        public PlayerType Type { get; internal set; }
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
        public PlayerId PlayerId { get; internal set; }
        public IImmutableList<PlayerView> Players { get; internal set; }
        public Team Team { get; internal set; }
        public Winner Winner { get; internal set; }
    }
}
