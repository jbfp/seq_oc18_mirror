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
    public sealed class ServerSentEventsController : ControllerBase, ISubscriber
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
            // We can't use HTTP headers with SSE so clients must provide player ID in query string.
            if (string.IsNullOrWhiteSpace(player))
            {
                Response.StatusCode = 401; // Unauthorized.
                return;
            }

            // To establish an SSE connection, we set the content-type appropriately.
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Content-Type"] = "text/event-stream";
            await Response.Body.FlushAsync(cancellationToken);

            var gameId = new GameId(id);

            // TODO: Use player ID.
            using (var subscription = _handler.Subscribe(gameId, this))
            {
                try
                {
                    // cancellationToken is signaled when the connection is closed. Task.Delay with
                    // infinite timeout will therefore wait for the connection to be closed before
                    // throwing OperationCanceledException and unsubscribing.
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        [NonAction]
        public async Task UpdateGameAsync(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            var services = Response.HttpContext.RequestServices;
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
                    gameEvent.Winner,
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
                await Response.WriteAsync($"event: {eventType}\n");
            }

            await Response.WriteAsync($"data: {data}\n");
            await Response.WriteAsync("\n");
            await Response.Body.FlushAsync();
        }
    }
}
