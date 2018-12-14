using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using Sequence.Core.GetGame;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class GetGameController : SequenceControllerBase
    {
        private readonly GetGameHandler _handler;
        private readonly ILogger _logger;

        public GetGameController(GetGameHandler handler, ILogger<GetGameController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/games/{id}")]
        public async Task<ActionResult<GetGameResult>> Get(
            [FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var view = await _handler.GetGameViewForPlayerAsync(gameId, PlayerId, cancellationToken);
            var result = new GetGameResult(view);
            return Ok(result);
        }
    }

    public sealed class GetGameResult
    {
        public GetGameResult(GameView view)
        {
            Game = view ?? throw new ArgumentNullException(nameof(view));
        }

        public GameView Game { get; }
    }
}
