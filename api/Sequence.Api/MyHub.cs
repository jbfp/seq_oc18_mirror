using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using System;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class GameEventDto
    {
        public PlayerId ByPlayerId { get; set; }
        public bool CardDrawn { get; set; }
        public Card CardUsed { get; set; }
        public Team? Chip { get; set; }
        public Coord Coord { get; set; }
        public int Index { get; set; }
        public PlayerId NextPlayerId { get; set; }
        public Seq Sequence { get; set; }
        public Team? Winner { get; set; }
    }

    public interface IMyHubClient
    {
        Task UpdateGame(GameEventDto gameEvent);
    }

    public sealed class MyHub : Hub<IMyHubClient>
    {
        private readonly ILogger _logger;

        public MyHub(ILogger<MyHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Subscribe(string gameId)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            var connectionId = Context.ConnectionId;
            var cancellationToken = Context.ConnectionAborted;
            _logger.LogInformation("User subscribing to {GameId}", gameId);
            await Groups.AddToGroupAsync(connectionId, gameId, cancellationToken);
        }

        public async Task Unsubscribe(string gameId)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            var connectionId = Context.ConnectionId;
            var cancellationToken = Context.ConnectionAborted;
            _logger.LogInformation("User unsubscribing from {GameId}", gameId);
            await Groups.RemoveFromGroupAsync(connectionId, gameId, cancellationToken);
        }
    }
}
