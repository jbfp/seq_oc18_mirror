using Microsoft.AspNetCore.Mvc;
using Sequence.AspNetCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGame
{
    public sealed class GetGameController : SequenceControllerBase
    {
        private readonly IGameProvider _provider;

        public GetGameController(IGameProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        [HttpGet("/games/{id:guid}")]
        public async Task<ActionResult> Get(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var game = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (game == null)
            {
                return NotFound();
            }

            var result = new
            {
                init = game.Init(Player),
                updates = game.GenerateForPlayer(Player)
            };

            return Ok(result);
        }
    }
}
