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
            _initialState = initialState;
            _gameEvents = gameEvents;
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
            var isInGame = playerIdx != -1;

            return new InitialGameState(
                boardType: FromIBoardType(state.BoardType),
                firstPlayerId: state.CurrentPlayerId,
                hand: isInGame ? state.PlayerHandByIdx[playerIdx] : null,
                numCardsInDeck: state.Deck.Count,
                playerHandle: isInGame ? state.PlayerHandleByIdx[playerIdx] : null,
                playerId: isInGame ? state.PlayerIdByIdx[playerIdx] : null,
                players: Enumerable.Range(0, state.NumberOfPlayers).Select(idx => new Player(
                    handle: state.PlayerHandleByIdx[idx],
                    id: state.PlayerIdByIdx[idx],
                    numberOfCards: state.PlayerHandByIdx[idx].Count,
                    // Shift teams so that the calling player always appears as red.
                    team: isInGame ? GetTeam(playerIdx, idx) : GetTeam(idx),
                    type: state.PlayerTypeByIdx[idx]
                )).ToImmutableList(),
                team: isInGame ? Team.Red : (Team?)null,
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

        public IEnumerable<GameUpdated> GenerateForObserver()
        {
            return GenerateForPlayer(-1);
        }

        private IEnumerable<GameUpdated> GenerateForPlayer(int playerIdx)
        {
            var state = _initialState;

            foreach (var gameEvent in _gameEvents)
            {
                var previousState = state;
                var currentState = previousState.Apply(gameEvent);
                var events = GetEvents(playerIdx, gameEvent, previousState, currentState);

                yield return new GameUpdated(
                    events.ToImmutableArray(),
                    gameEvent.Index);

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
            var byPlayerId = gameEvent.ByPlayerId;
            var idx = _initialState.PlayerIdByIdx.IndexOf(byPlayerId);
            var team = isInGame ? GetTeam(playerIdx, idx) : GetTeam(idx);

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
                yield return new GameEnded(team);
            }
        }

        private Team GetTeam(int basePlayerIdx, int playerIdx)
        {
            var numPlayers = _initialState.NumberOfPlayers;
            var index = (basePlayerIdx + playerIdx) % numPlayers;
            var teamsByIndex = _initialState.PlayerTeamByIdx;
            return teamsByIndex[index];
        }

        private Team GetTeam(int playerIdx)
        {
            return _initialState.PlayerTeamByIdx[playerIdx];
        }

        private static BoardType FromIBoardType(IBoardType boardType)
        {
            return boardType switch
            {
                OneEyedJackBoard _ => BoardType.OneEyedJack,
                SequenceBoard _ => BoardType.Sequence,
                _ => throw new ArgumentOutOfRangeException(nameof(boardType), boardType.GetType(), null),
            };
        }
    }
}
