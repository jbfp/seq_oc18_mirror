using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using Sequence.Core.Notifications;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    [ApiController]
    public sealed class ServerSentEventsController : ControllerBase
    {
        private readonly SubscriptionHandler _handler;
        private readonly ILogger _logger;

        public ServerSentEventsController(
            SubscriptionHandler handler,
            ILogger<ServerSentEventsController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/games/{id:guid}/stream")]
        public async Task Get([FromRoute] Guid id, [FromQuery] string player, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(player))
            {
                Response.StatusCode = 401;
                return;
            }

            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Content-Type"] = "text/event-stream";
            await Response.Body.FlushAsync(cancellationToken);

            var gameId = new GameId(id);
            var subscriber = new Subscriber(Response);

            // TODO: Use player ID.
            using (var subscription = _handler.Subscribe(gameId, subscriber))
            {
                await cancellationToken.WaitAsync();
            }
        }

        private sealed class Subscriber : ISubscriber
        {
            private readonly HttpResponse _response;

            public Subscriber(HttpResponse response)
            {
                _response = response ?? throw new ArgumentNullException(nameof(response));
            }

            public async Task UpdateGameAsync(GameEvent gameEvent)
            {
                if (gameEvent == null)
                {
                    throw new ArgumentNullException(nameof(gameEvent));
                }

                var services = _response.HttpContext.RequestServices;
                var formatter = services.GetRequiredService<JsonOutputFormatter>();

                using (var writer = new StringWriter())
                {
                    formatter.WriteObject(writer, new
                    {
                        gameEvent.ByPlayerId,
                        CardDrawn = gameEvent.CardDrawn != null, // Deliberately excluding which card was drawn.
                        gameEvent.CardUsed,
                        gameEvent.Chip,
                        gameEvent.Coord,
                        gameEvent.Index,
                        gameEvent.NextPlayerId,
                        gameEvent.Sequence,
                    });

                    await WriteEventAsync("game-updated", writer.ToString());
                }
            }

            private async Task WriteEventAsync(string eventType, string data)
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(eventType))
                {
                    await _response.WriteAsync($"event: {eventType}\n");
                }

                await _response.WriteAsync($"data: {data}\n");
                await _response.WriteAsync("\n");
                await _response.Body.FlushAsync();
            }
        }
    }
}
