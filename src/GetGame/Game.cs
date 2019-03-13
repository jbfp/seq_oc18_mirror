using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.GetGame
{
    public sealed class Game
    {
        private readonly GameState _initialState;
        private readonly GameEvent[] _gameEvents;

        public Game(GameState initialState, params GameEvent[] gameEvents)
        {
            _initialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            _gameEvents = gameEvents ?? throw new ArgumentNullException(nameof(gameEvents));
        }

        public InitialGameState Init(PlayerHandle playerHandle)
        {
            return Init(_initialState.PlayerHandleByIdx.IndexOf(playerHandle));
        }

        public InitialGameState Init(PlayerId playerId)
        {
            return Init(_initialState.PlayerIdByIdx.IndexOf(playerId));
        }

        private InitialGameState Init(int playerIdx)
        {
            var state = _initialState;

            return new InitialGameState(
                boardType: FromIBoardType(state.BoardType),
                firstPlayerId: state.CurrentPlayerId,
                hand: state.PlayerHandByIdx[playerIdx],
                numCardsInDeck: state.Deck.Count,
                playerHandle: state.PlayerHandleByIdx[playerIdx],
                playerId: state.PlayerIdByIdx[playerIdx],
                players: Enumerable.Range(0, state.NumberOfPlayers).Select(idx => new Player
                {
                    Handle = state.PlayerHandleByIdx[idx],
                    Id = state.PlayerIdByIdx[idx],
                    NumberOfCards = state.PlayerHandByIdx[idx].Count,
                    // Shift teams so that the calling player always appears as red.
                    Team = GetTeam(playerIdx, idx),
                    Type = state.PlayerTypeByIdx[idx],
                }).ToImmutableList(),
                team: Team.Red,
                winCondition: state.WinCondition
            );
        }

        public IEnumerable<GameUpdated> GenerateForPlayer(PlayerHandle playerHandle)
        {
            return GenerateForPlayer(_initialState.PlayerHandleByIdx.IndexOf(playerHandle));
        }

        public IEnumerable<GameUpdated> GenerateForPlayer(PlayerId playerId)
        {
            return GenerateForPlayer(_initialState.PlayerIdByIdx.IndexOf(playerId));
        }

        private IEnumerable<GameUpdated> GenerateForPlayer(int playerIdx)
        {
            var state = _initialState;

            foreach (var gameEvent in _gameEvents)
            {
                var previousState = state;
                var currentState = previousState.Apply(gameEvent);
                var events = GetEvents(playerIdx, gameEvent, previousState, currentState);

                yield return new GameUpdated
                {
                    GameEvents = events.ToArray(),
                    Version = gameEvent.Index,
                };

                state = currentState;
            }
        }

        private IEnumerable<IGameEvent> GetEvents(
            int playerIdx,
            GameEvent gameEvent,
            Sequence.GameState previousState,
            Sequence.GameState currentState)
        {
            var isInGame = playerIdx != -1;

            yield return new CardDiscarded(gameEvent.ByPlayerId, gameEvent.CardUsed);

            if (gameEvent.CardDrawn != null)
            {
                var byPlayerIdx = _initialState.PlayerIdByIdx.IndexOf(gameEvent.ByPlayerId);
                var cardDrawn = byPlayerIdx == playerIdx ? gameEvent.CardDrawn : null;
                yield return new CardDrawn(gameEvent.ByPlayerId, cardDrawn);
            }

            if (previousState.Deck.Count == 0)
            {
                yield return new DeckShuffled(currentState.Deck.Count);
            }

            if (gameEvent.Coord.Equals(new Coord(-1, -1)))
            {
                // Dead card exchanged = card discarded + card drawn.
            }
            else
            {
                var previousHand = isInGame
                    ? previousState.PlayerHandByIdx[playerIdx]
                    : ImmutableList<Card>.Empty;

                var currentHand = isInGame
                    ? currentState.PlayerHandByIdx[playerIdx]
                    : ImmutableList<Card>.Empty;

                if (gameEvent.Chip.HasValue)
                {
                    var byPlayerId = gameEvent.ByPlayerId;
                    var idx = _initialState.PlayerIdByIdx.IndexOf(byPlayerId);
                    var team = GetTeam(playerIdx, idx);

                    yield return new ChipAdded(gameEvent.Coord, team);

                    foreach (var sequence in gameEvent.Sequences)
                    {
                        yield return new SequenceCreated(
                            new Seq(team, sequence.Coords)
                        );
                    }

                    var newDeadCards = currentState.DeadCards
                        .Intersect(currentHand)
                        .Except(previousState.DeadCards
                            .Intersect(previousHand));

                    foreach (var card in newDeadCards)
                    {
                        yield return new CardDied(card);
                    }
                }
                else
                {
                    yield return new ChipRemoved(gameEvent.Coord);

                    var revivedCards = previousState.DeadCards
                        .Intersect(previousHand)
                        .Except(currentState.DeadCards
                            .Intersect(currentHand));

                    foreach (var card in revivedCards)
                    {
                        yield return new CardRevived(card);
                    }
                }
            }

            if (gameEvent.NextPlayerId != null)
            {
                yield return new TurnEnded(gameEvent.NextPlayerId);
            }

            if (gameEvent.Winner.HasValue)
            {
                yield return new GameEnded(gameEvent.Winner.Value);
            }
        }

        private Team GetTeam(int basePlayerIdx, int playerIdx)
        {
            var numPlayers = _initialState.NumberOfPlayers;
            var index = (basePlayerIdx + playerIdx) % numPlayers;
            var teamsByIndex = _initialState.PlayerTeamByIdx;
            return teamsByIndex[index];
        }

        private static BoardType FromIBoardType(IBoardType boardType)
        {
            switch (boardType)
            {
                case OneEyedJackBoard b1:
                    return BoardType.OneEyedJack;

                case SequenceBoard b2:
                    return BoardType.Sequence;

                default:
                    throw new ArgumentOutOfRangeException(nameof(boardType), boardType.GetType(), null);
            }
        }
    }
}
