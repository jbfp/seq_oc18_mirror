using Microsoft.AspNetCore.SignalR;
using Sequence.Core;
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
        public async Task Subscribe(string gameId)
        {
            var connectionId = Context.ConnectionId;
            var cancellationToken = Context.ConnectionAborted;
            await Groups.AddToGroupAsync(connectionId, gameId, cancellationToken);
        }

        public async Task Unsubscribe(string gameId)
        {
            var connectionId = Context.ConnectionId;
            var cancellationToken = Context.ConnectionAborted;
            await Groups.RemoveFromGroupAsync(connectionId, gameId, cancellationToken);
        }
    }
}
