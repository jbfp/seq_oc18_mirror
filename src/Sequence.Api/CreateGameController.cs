using Microsoft.AspNetCore.Mvc;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class CreateGameController : SequenceControllerBase
    {
        private readonly CreateGameHandler _handler;

        public CreateGameController(CreateGameHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [HttpPost("/api/games")]
        public async Task<ActionResult> Post(
            [FromBody] CreateGameForm form,
            CancellationToken cancellationToken)
        {
            var player1 = PlayerId;
            var player2 = new PlayerId(form.Opponent);
            var gameId = await _handler.CreateGameAsync(player1, player2, cancellationToken);
            return Created($"/api/games/{gameId}", new { gameId });
        }
    }

    public sealed class CreateGameForm
    {
        [Required]
        public string Opponent { get; set; }
    }
}
