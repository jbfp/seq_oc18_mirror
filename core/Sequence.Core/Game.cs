using Sequence.Core.Boards;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.Core
{
    public sealed class Game
    {
        private readonly IBoardType _boardType;
        private readonly Deck _deck;
        private readonly int _numSequencesToWin;
        private readonly ImmutableArray<PlayerHandle> _playerHandleByIdx;
        private readonly ImmutableArray<PlayerId> _playerIdByIdx;
        private readonly ImmutableArray<PlayerType> _playerTypeByIdx;
        private readonly ImmutableArray<Team> _teamByIdx;

        private ImmutableDictionary<Coord, Team> _chips;
        private ImmutableArray<IImmutableList<Card>> _handByIdx;
        private ImmutableStack<Card> _discards;
        private PlayerId _currentPlayerId;
        private Coord? _latestMoveAt;
        private ImmutableArray<Seq> _sequences;
        private int _version;
        private Team? _winner;

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

            switch (init.BoardType)
            {
                case BoardType.OneEyedJack:
                    _boardType = new OneEyedJackBoard();
                    break;

                case BoardType.Sequence:
                    _boardType = new SequenceBoard();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(init.BoardType), init.BoardType, null);
            }

            _chips = ImmutableDictionary<Coord, Team>.Empty;
            _currentPlayerId = init.FirstPlayerId;
            _deck = new Deck(init.Seed, init.Players.Count);
            _discards = ImmutableStack<Card>.Empty;
            _handByIdx = _deck.DealHands().ToImmutableArray();
            _numSequencesToWin = init.NumberOfSequencesToWin;
            _playerHandleByIdx = init.Players.Select(p => p.Handle).ToImmutableArray();
            _playerIdByIdx = init.Players.Select(p => p.Id).ToImmutableArray();
            _playerTypeByIdx = init.Players.Select(p => p.Type).ToImmutableArray();
            _sequences = ImmutableArray<Seq>.Empty;

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

        private ImmutableHashSet<Coord> CoordsInSequence => _sequences
            .SelectMany(s => s.Coords)
            .ToImmutableHashSet();

        public void Apply(GameEvent gameEvent)
        {
            if (_version == gameEvent.Index)
            {
                return;
            }

            if (_version > gameEvent.Index)
            {
                throw new InvalidOperationException($"Game event cannot be applied. Index is {_version}, game event index is {gameEvent.Index}.");
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

            if (gameEvent.Chip == null)
            {
                _chips = _chips.Remove(gameEvent.Coord);
            }
            else
            {
                _chips = _chips.Add(gameEvent.Coord, gameEvent.Chip.Value);
            }

            _currentPlayerId = gameEvent.NextPlayerId;
            _latestMoveAt = gameEvent.Coord;
            _version = gameEvent.Index;
            _winner = gameEvent.Winner;

            if (gameEvent.Sequence != null)
            {
                _sequences = _sequences.Add(gameEvent.Sequence);
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
                if (!_chips.ContainsKey(coord))
                {
                    throw new PlayCardFailedException(PlayCardError.CoordIsEmpty);
                }

                if (_chips.TryGetValue(coord, out var chip) && chip == _teamByIdx[playerIdx])
                {
                    throw new PlayCardFailedException(PlayCardError.ChipBelongsToPlayerTeam);
                }

                if (CoordsInSequence.Contains(coord))
                {
                    throw new PlayCardFailedException(PlayCardError.ChipIsPartOfSequence);
                }

                return new GameEvent
                {
                    ByPlayerId = playerId,
                    CardDrawn = _deck.Top,
                    CardUsed = card,
                    Chip = null,
                    Coord = coord,
                    Index = _version + 1,
                    NextPlayerId = _playerIdByIdx[(playerIdx + 1) % _playerIdByIdx.Length],
                };
            }
            else
            {
                if (_chips.ContainsKey(coord))
                {
                    throw new PlayCardFailedException(PlayCardError.CoordIsOccupied);
                }

                if (!_boardType.Board.Matches(coord, card))
                {
                    throw new PlayCardFailedException(PlayCardError.CardDoesNotMatchCoord);
                }

                var team = _teamByIdx[playerIdx];
                var sequence = _boardType.Board.GetSequence(
                    chips: _chips.Add(coord, team),
                    CoordsInSequence,
                    coord, team);

                Team? winnerTeam = null;

                if (sequence != null)
                {
                    // Test for win condition:
                    var numSequencesForTeam = _sequences
                        .Add(sequence)
                        .Count(seq => seq.Team == team);

                    if (numSequencesForTeam == _numSequencesToWin)
                    {
                        winnerTeam = team;
                    }
                }

                var nextPlayerId = winnerTeam == null ? _playerIdByIdx[(playerIdx + 1) % _playerIdByIdx.Length] : null;

                return new GameEvent
                {
                    ByPlayerId = playerId,
                    CardDrawn = _deck.Top,
                    CardUsed = card,
                    Chip = team,
                    Coord = coord,
                    Index = _version + 1,
                    NextPlayerId = nextPlayerId,
                    Sequence = sequence,
                    Winner = winnerTeam,
                };
            }
        }

        public IImmutableList<Move> GetMovesForPlayer(PlayerId playerId)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            var idx = _playerIdByIdx.IndexOf(playerId);

            if (idx < 0)
            {
                return ImmutableList<Move>.Empty;
            }

            var hand = _handByIdx[idx];
            var team = _teamByIdx[idx];
            var moves = ImmutableList.CreateBuilder<Move>();
            var coordsInSequence = CoordsInSequence;

            foreach (var card in hand)
            {
                if (card.IsOneEyedJack())
                {
                    foreach (var chip in _chips)
                    {
                        var coord = chip.Key;
                        var isNotOwnTeam = chip.Value != team;
                        var isNotPartOfSequence = !coordsInSequence.Contains(coord);

                        if (isNotOwnTeam && isNotPartOfSequence)
                        {
                            moves.Add(new Move(card, coord));
                        }
                    }
                }
                else if (card.IsTwoEyedJack())
                {
                    foreach (var (row, y) in _boardType.Board.Select((row, y) => (row, y)))
                    {
                        foreach (var (cell, x) in row.Select((cell, x) => (cell, x)))
                        {
                            var coord = new Coord(x, y);
                            var isNotCorner = cell != null;
                            var isFree = !_chips.ContainsKey(coord);

                            if (isNotCorner && isFree)
                            {
                                moves.Add(new Move(card, coord));
                            }
                        }
                    }
                }
                else
                {
                    var tile = new Tile(card.Suit, card.Rank);

                    if (_boardType.CoordsByTile.TryGetValue(tile, out var coords))
                    {
                        if (!_chips.ContainsKey(coords.Item1))
                        {
                            moves.Add(new Move(card, coords.Item1));
                        }

                        if (!_chips.ContainsKey(coords.Item2))
                        {
                            moves.Add(new Move(card, coords.Item2));
                        }
                    }
                }
            }

            return moves.ToImmutable();
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
            var coordsInSequence = CoordsInSequence;

            var view = new GameView
            {
                Board = _boardType.Board,
                Chips = _chips.Select(c => new ChipView
                {
                    Coord = c.Key,
                    IsLocked = coordsInSequence.Contains(c.Key),
                    Team = c.Value,
                }).ToImmutableArray(),
                CurrentPlayerId = _currentPlayerId,
                Discards = _discards.ToImmutableArray(),
                LatestMoveAt = _latestMoveAt,
                NumberOfCardsInDeck = _deck.Count,
                NumberOfSequencesToWin = _numSequencesToWin,
                Players = _playerIdByIdx.Select((p, i) => new PlayerView
                {
                    Id = p,
                    Handle = _playerHandleByIdx[i],
                    NumberOfCards = _handByIdx[i].Count,
                    Team = _teamByIdx[i],
                    Type = _playerTypeByIdx[i],
                }).ToImmutableArray(),
                Version = _version,
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
        ChipIsPartOfSequence,
    }

    public enum Team
    {
        Red, Green, Blue,
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
        public ImmutableArray<ImmutableArray<Tile>> Board { get; internal set; }
        public IImmutableList<ChipView> Chips { get; internal set; }
        public PlayerId CurrentPlayerId { get; internal set; }
        public IImmutableList<Card> Discards { get; internal set; }
        public IImmutableList<Card> Hand { get; internal set; }
        public Coord? LatestMoveAt { get; internal set; }
        public int NumberOfCardsInDeck { get; internal set; }
        public int NumberOfSequencesToWin { get; internal set; }
        public PlayerId PlayerId { get; internal set; }
        public IImmutableList<PlayerView> Players { get; internal set; }
        public Team Team { get; internal set; }
        public int Version { get; internal set; }
        public Team? Winner { get; internal set; }
    }
}
