using Microsoft.AspNetCore.SignalR;
using Sequence.RealTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.PlayCard
{
    public sealed class PlayCardHandler
    {
        private readonly IGameStateProvider _provider;
        private readonly IGameEventStore _store;
        private readonly IHubContext<GameHub, IGameHubClient> _hub;

        public PlayCardHandler(
            IGameStateProvider provider,
            IGameEventStore store,
            IHubContext<GameHub, IGameHubClient> hub)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
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
            var newState = new GetGame.GameState(state, gameEvent);

            var _ = Task.Run(() =>
            {
                async Task SendUpdates(PlayerId playerId)
                {
                    var updates = newState.GenerateForPlayer(playerId);
                    var groupName = playerId.ToString();
                    var client = _hub.Clients.Group(groupName);

                    foreach (var update in updates)
                    {
                        await client.UpdateGame(update);
                    }
                }

                var tasks = state
                    .PlayerIdByIdx
                    .AsParallel()
                    .Where(playerId => playerId != gameEvent.ByPlayerId)
                    .Select(SendUpdates);

                return Task.WhenAll(tasks);
            });

            return newState.GenerateForPlayer(gameEvent.ByPlayerId);
        }
    }
}
