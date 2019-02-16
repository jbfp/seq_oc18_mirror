using Microsoft.AspNetCore.SignalR;
using Sequence.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class SignalRGameEventStoreDecorator : IGameEventStore
    {
        private readonly IGameEventStore _store;
        private readonly IHubContext<MyHub, IMyHubClient> _hubContext;

        public SignalRGameEventStoreDecorator(
            IGameEventStore store,
            IHubContext<MyHub, IMyHubClient> hubContext)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
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
            await group.UpdateGame(dto);
        }
    }
}
