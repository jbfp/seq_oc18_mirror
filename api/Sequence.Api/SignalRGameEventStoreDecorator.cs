using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class SignalRGameEventStoreDecorator : IGameEventStore
    {
        private readonly IGameEventStore _store;
        private readonly IHubContext<GameHub, IGameHubClient> _hubContext;
        private readonly ILogger _logger;

        public SignalRGameEventStoreDecorator(
            IGameEventStore store,
            IHubContext<GameHub, IGameHubClient> hubContext,
            ILogger<SignalRGameEventStoreDecorator> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddEventAsync(
            GameId gameId,
            GameEvent gameEvent,
            CancellationToken cancellationToken)
        {
            await _store.AddEventAsync(gameId, gameEvent, cancellationToken);

            var dto = new GameEventDto
            {
                ByPlayerId = gameEvent.ByPlayerId,
                CardDrawn = gameEvent.CardDrawn != null, // Deliberately excluding which card was drawn.
                CardUsed = gameEvent.CardUsed,
                Chip = gameEvent.Chip,
                Coord = gameEvent.Coord,
                Index = gameEvent.Index,
                NextPlayerId = gameEvent.NextPlayerId,
                Sequence = gameEvent.Sequence,
                Winner = gameEvent.Winner,
            };

            var groupName = gameId.ToString();
            var group = _hubContext.Clients.Group(groupName);
            _logger.LogInformation("Sending game update for {GameId}", gameId);
            await group.UpdateGame(dto);
        }
    }
}
