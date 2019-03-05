using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.RealTime
{
    public sealed class SignalRNotifier : BackgroundService
    {
        private readonly IObservable<(GameId, GameEvent)> _observable;
        private readonly IHubContext<GameHub, IGameHubClient> _hubContext;
        private readonly ILogger _logger;

        public SignalRNotifier(
            IObservable<(GameId, GameEvent)> observable,
            IHubContext<GameHub, IGameHubClient> hubContext,
            ILogger<SignalRNotifier> logger)
        {
            _observable = observable ?? throw new ArgumentNullException(nameof(observable));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _observable
                .SelectMany(tuple =>
                {
                    var (gameId, gameEvent) = tuple;
                    var groupName = gameId.ToString();
                    var group = _hubContext.Clients.Group(groupName);
                    var dto = Map(gameEvent);
                    _logger.LogInformation("Sending game update for {GameId}", gameId);
                    return Observable.FromAsync(_ => group.UpdateGame(dto));
                })
                .DefaultIfEmpty(Unit.Default)
                .RunAsync(stoppingToken);
        }

        private static GameEventDto Map(GameEvent gameEvent)
        {
            return new GameEventDto
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
        }
    }
}
