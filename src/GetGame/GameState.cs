using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.GetGame
{
    public sealed class GameState
    {
        private readonly Sequence.GameState _initialState;
        private readonly GameEvent[] _gameEvents;

        public GameState(Sequence.GameState initialState, params GameEvent[] gameEvents)
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
                    Team = state.PlayerTeamByIdx[idx],
                    Type = state.PlayerTypeByIdx[idx],
                }).ToImmutableList(),
                team: state.PlayerTeamByIdx[playerIdx],
                winCondition: state.WinCondition
            );
        }

        public IEnumerable<GameEventBase> GenerateForPlayer(PlayerHandle playerHandle)
        {
            return GenerateForPlayer(_initialState.PlayerHandleByIdx.IndexOf(playerHandle));
        }

        public IEnumerable<GameEventBase> GenerateForPlayer(PlayerId playerId)
        {
            return GenerateForPlayer(_initialState.PlayerIdByIdx.IndexOf(playerId));
        }

        private IEnumerable<GameEventBase> GenerateForPlayer(int playerIdx)
        {
            var version = 1;
            var state = _initialState;

            foreach (var gameEvent in _gameEvents)
            {
                var previousState = state;
                var currentState = previousState.Apply(gameEvent);

                yield return new CardDiscarded(gameEvent.ByPlayerId, gameEvent.CardUsed, version++);

                if (gameEvent.CardDrawn != null)
                {
                    var byPlayerIdx = state.PlayerIdByIdx.IndexOf(gameEvent.ByPlayerId);
                    var cardDrawn = byPlayerIdx == playerIdx ? gameEvent.CardDrawn : null;
                    yield return new CardDrawn(gameEvent.ByPlayerId, cardDrawn, version++);
                }

                if (previousState.Deck.Count == 0)
                {
                    yield return new DeckShuffled(currentState.Deck.Count, version++);
                }

                if (gameEvent.Coord.Equals(new Coord(-1, -1)))
                {
                    // Dead card exchanged = card discarded + card drawn.
                }
                else
                {
                    var previousHand = previousState.PlayerHandByIdx[playerIdx];
                    var currentHand = currentState.PlayerHandByIdx[playerIdx];

                    if (gameEvent.Chip.HasValue)
                    {
                        yield return new ChipAdded(gameEvent.Coord, gameEvent.Chip.Value, version++);

                        foreach (var sequence in gameEvent.Sequences)
                        {
                            yield return new SequenceCreated(sequence, version++);
                        }

                        var newDeadCards = currentState.DeadCards
                            .Intersect(currentHand)
                            .Except(previousState.DeadCards
                                .Intersect(previousHand));

                        foreach (var card in newDeadCards)
                        {
                            yield return new CardDied(card, version++);
                        }
                    }
                    else
                    {
                        yield return new ChipRemoved(gameEvent.Coord, version++);

                        var revivedCards = previousState.DeadCards
                            .Intersect(previousHand)
                            .Except(currentState.DeadCards
                                .Intersect(currentHand));

                        foreach (var card in revivedCards)
                        {
                            yield return new CardRevived(card, version++);
                        }
                    }
                }

                if (gameEvent.NextPlayerId != null)
                {
                    yield return new TurnEnded(gameEvent.NextPlayerId, version++);
                }

                if (gameEvent.Winner.HasValue)
                {
                    yield return new GameEnded(gameEvent.Winner.Value, version++);
                }

                state = currentState;
            }
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
