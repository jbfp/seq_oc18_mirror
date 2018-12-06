using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sequence.Core;
using Sequence.Core.Notifications;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    [ApiController]
    public sealed class ServerSentEventsController : ControllerBase
    {
        private readonly SubscriptionHandler _handler;

        public ServerSentEventsController(SubscriptionHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [HttpGet("/games/{id}/stream")]
        public async Task Get([FromRoute] string id, [FromQuery] string playerId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(playerId))
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

            public async Task UpdateGameAsync(int version)
            {
                await WriteEventAsync("game-updated", version.ToString());
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
