using Microsoft.AspNetCore.Mvc;
using Sequence.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameEvents
{
    public sealed class GetGameEventsController : SequenceControllerBase
    {
        private readonly IGameEventGeneratorProvider _provider;

        public GetGameEventsController(IGameEventGeneratorProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        [HttpGet("/games/{id:guid}/game-events")]
        public async Task<ActionResult> Get(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var generator = await _provider.GetGameEventGeneratorByIdAsync(
                gameId, cancellationToken);

            if (generator == null)
            {
                return NotFound();
            }

            var result = new
            {
                events = generator.GenerateForPlayer(Player)
            };

            return Ok(result);
        }
    }
}
