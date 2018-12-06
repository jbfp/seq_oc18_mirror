using Microsoft.AspNetCore.Mvc;
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

        public GetGameController(GetGameHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
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
