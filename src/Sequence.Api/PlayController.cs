using Microsoft.AspNetCore.Mvc;
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

        public PlayController(PlayHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [HttpPost("/games/{id}")]
        [PlayCardFailedExceptionFilter]
        public async Task<ActionResult<PlayCardResult>> Post(
            [FromRoute] string id,
            [FromBody] PlayCardForm form,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var coord = new Coord(form.Column.Value, form.Row.Value);
            var result = await _handler.PlayCardAsync(gameId, PlayerId, form.Card, coord, cancellationToken);
            return Ok(result);
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
