using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.PlayCard
{
    public sealed class PlayCardHandler
    {
        private readonly IGameStateProvider _provider;
        private readonly IGameEventStore _store;
        private readonly IRealTimeContext _realTime;

        public PlayCardHandler(
            IGameStateProvider provider,
            IGameEventStore store,
            IRealTimeContext realTime)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _realTime = realTime ?? throw new ArgumentNullException(nameof(realTime));
        }

        public async Task<IImmutableList<Move>> GetMovesForPlayerAsync(
            GameId gameId,
            PlayerId playerId,
            CancellationToken cancellationToken)
        {
            var state = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (state == null)
            {
                throw new GameNotFoundException();
            }

            return Moves.GenerateMoves(state, playerId);
        }

        public Task<IEnumerable<GameUpdated>> PlayCardAsync(
            GameId gameId,
            PlayerHandle player,
            Card card,
            Coord coord,
            CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return DoThing(
                gameId,
                state => Game.PlayCard(state, player, card, coord),
                cancellationToken);
        }

        public Task<IEnumerable<GameUpdated>> PlayCardAsync(
            GameId gameId,
            PlayerId player,
            Card card,
            Coord coord,
            CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return DoThing(
                gameId,
                state => Game.PlayCard(state, player, card, coord),
                cancellationToken);
        }

        public Task<IEnumerable<GameUpdated>> ExchangeDeadCardAsync(
            GameId gameId,
            PlayerHandle player,
            Card deadCard,
            CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (deadCard == null)
            {
                throw new ArgumentNullException(nameof(deadCard));
            }

            return DoThing(
                gameId,
                state => Game.ExchangeDeadCard(state, player, deadCard),
                cancellationToken);
        }

        public Task<IEnumerable<GameUpdated>> ExchangeDeadCardAsync(
            GameId gameId,
            PlayerId player,
            Card deadCard,
            CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (deadCard == null)
            {
                throw new ArgumentNullException(nameof(deadCard));
            }

            return DoThing(
                gameId,
                state => Game.ExchangeDeadCard(state, player, deadCard),
                cancellationToken);
        }

        private async Task<IEnumerable<GameUpdated>> DoThing(
            GameId gameId,
            Func<GameState, GameEvent> doThing,
            CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var state = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (state == null)
            {
                throw new GameNotFoundException();
            }

            var gameEvent = doThing(state);
            await _store.AddEventAsync(gameId, gameEvent, cancellationToken);
            var newState = new GetGame.Game(state, gameEvent);

            var _ = Task.Run(() =>
            {
                var tasks = state
                    .PlayerIdByIdx
                    .AsParallel()
                    .Where(playerId => playerId != gameEvent.ByPlayerId)
                    .Select(playerId => (playerId, updates: newState.GenerateForPlayer(playerId)))
                    .Select(t => _realTime.SendGameUpdatesAsync(t.playerId, t.updates));

                return Task.WhenAll(tasks);
            });

            return newState.GenerateForPlayer(gameEvent.ByPlayerId);
        }
    }
}
