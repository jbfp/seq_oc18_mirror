using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public CreateGameController(CreateGameHandler handler, ILogger<CreateGameController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("/games")]
        public async Task<ActionResult> Post(
            [FromBody] CreateGameForm form,
            CancellationToken cancellationToken)
        {
            var player1 = PlayerId;
            var player2 = new PlayerId(form.Opponent);

            GameId gameId;

            _logger.LogInformation("Attempting to create game for {Player1} vs {Player2}", player1, player2);

            try
            {
                gameId = await _handler.CreateGameAsync(player1, player2, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex,
                    "Failed to create game for {Player1} vs {Player2}: {Error}",
                    player1, player2, ex.Message);

                return BadRequest(new { error = ex.Message });
            }

            _logger.LogInformation(
                "Successfully created game with ID {GameId} for {Player1} vs {Player2}",
                gameId, player1, player2);

            return Created($"/games/{gameId}", new { gameId });
        }
    }

    public sealed class CreateGameForm
    {
        [Required]
        public string Opponent { get; set; }
    }
}
