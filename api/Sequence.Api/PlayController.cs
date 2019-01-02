using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using Sequence.Core.Play;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class PlayController : SequenceControllerBase
    {
        private readonly PlayHandler _handler;
        private readonly ILogger _logger;

        public PlayController(PlayHandler handler, ILogger<PlayController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("/games/{id:guid}")]
        [PlayCardFailedExceptionFilter]
        public async Task<ActionResult<GameEvent>> Post(
            [FromRoute] Guid id,
            [FromBody] PlayCardForm form,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var coord = new Coord(form.Column.Value, form.Row.Value);
            var gameEvent = await _handler.PlayCardAsync(gameId, Player, form.Card, coord, cancellationToken);
            return Ok(gameEvent);
        }
    }

    public sealed class PlayCardForm
    {
        [Required]
        public Card Card { get; set; }

        [Required]
        public int? Column { get; set; }

        [Required]
        public int? Row { get; set; }
    }
}
