using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.GetGameEvents
{
    public sealed class GameEventGenerator
    {
        private readonly GameInit _init;
        private readonly GameEvent[] _gameEvents;

        public GameEventGenerator(GameInit init, params GameEvent[] gameEvents)
        {
            _init = init ?? throw new ArgumentNullException(nameof(init));
            _gameEvents = gameEvents ?? throw new ArgumentNullException(nameof(gameEvents));
        }

        public IEnumerable<GameEventBase> GenerateForPlayer(PlayerHandle playerHandle)
        {
            var state = new GameState(_init);
            var playerIdx = playerHandle == null ? -1 : state.PlayerHandleByIdx.IndexOf(playerHandle);
            return GenerateForPlayer(state, playerIdx);
        }

        public IEnumerable<GameEventBase> GenerateForPlayer(PlayerId playerId)
        {
            var state = new GameState(_init);
            var playerIdx = playerId == null ? -1 : state.PlayerIdByIdx.IndexOf(playerId);
            return GenerateForPlayer(state, playerIdx);
        }

        private IEnumerable<GameEventBase> GenerateForPlayer(GameState state, int playerIdx)
        {
            var version = 0;
            var isInGame = playerIdx >= 0;

            yield return new GameStarted(
                boardType: FromIBoardType(state.BoardType),
                firstPlayerId: state.CurrentPlayerId,
                hand: isInGame ? state.PlayerHandByIdx[playerIdx] : default,
                numCardsInDeck: state.Deck.Count,
                playerHandle: isInGame ? state.PlayerHandleByIdx[playerIdx] : default,
                playerId: isInGame ? state.PlayerIdByIdx[playerIdx] : default,
                players: Enumerable.Range(0, state.NumberOfPlayers).Select(idx => new Player
                {
                    Handle = state.PlayerHandleByIdx[idx],
                    Id = state.PlayerIdByIdx[idx],
                    NumberOfCards = state.PlayerHandByIdx[idx].Count,
                    Team = state.PlayerTeamByIdx[idx],
                    Type = state.PlayerTypeByIdx[idx],
                }).ToImmutableList(),
                team: isInGame ? state.PlayerTeamByIdx[playerIdx] : default,
                winCondition: state.WinCondition,
                version++
            );

            foreach (var gameEvent in _gameEvents)
            {
                var previousState = state;
                var currentState = previousState.Apply(gameEvent);

                yield return new CardDiscarded(gameEvent.ByPlayerId, gameEvent.CardUsed, version++);

                if (gameEvent.CardDrawn != null)
                {
                    yield return new CardDrawn(gameEvent.ByPlayerId, gameEvent.CardDrawn, version++);
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
                    var previousHand = isInGame
                        ? previousState.PlayerHandByIdx[playerIdx]
                        : ImmutableList<Card>.Empty;

                    var currentHand = isInGame
                        ? currentState.PlayerHandByIdx[playerIdx]
                        : ImmutableList<Card>.Empty;

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
