using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sequence.Core;
using Sequence.Core.Notifications;
using System;
using System.IO;
using System.Text;
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

        [HttpGet("/api/games/{id}/stream")]
        public void Get([FromRoute] string id, [FromQuery] string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                Response.StatusCode = 401;
                return;
            }

            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["X-Accel-Buffering"] = "no";
            Response.Body.Flush();

            var gameId = new GameId(id);
            var subscriber = new Subscriber(Response.Body);

            // TODO: Use player ID.
            using (var subscription = _handler.Subscribe(gameId, subscriber))
            {
                // Wait until connection is closed.
                HttpContext.RequestAborted.WaitHandle.WaitOne();
            }
        }

        private sealed class Subscriber : ISubscriber
        {
            private readonly Stream _stream;

            public Subscriber(Stream stream)
            {
                _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            }

            public async Task UpdateGameAsync(int version)
            {
                await WriteEventAsync("game-updated", version.ToString());
            }

            private async Task WriteEventAsync(string eventType, string data = "")
            {
                using (var writer = new StreamWriter(_stream, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    if (!string.IsNullOrEmpty(eventType))
                    {
                        await writer.WriteLineAsync($"event:{eventType}");
                    }

                    await writer.WriteLineAsync($"data:{data}");
                    await writer.WriteLineAsync("\n");
                    await writer.FlushAsync();
                }
            }
        }
    }
}
