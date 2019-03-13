using Microsoft.AspNetCore.SignalR;
using Sequence.PlayCard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sequence.RealTime
{
    public sealed class GameHubRealTimeContext : IRealTimeContext
    {
        private readonly IHubContext<GameHub, IGameHubClient> _hub;

        public GameHubRealTimeContext(IHubContext<GameHub, IGameHubClient> hub)
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        public async Task SendGameUpdatesAsync(PlayerId playerId, IEnumerable<GameUpdated> updates)
        {
            var groupName = playerId.ToString();
            var clients = _hub.Clients.Group(groupName);

            foreach (var update in updates)
            {
                await clients.UpdateGame(update);
            }
        }
    }
}
