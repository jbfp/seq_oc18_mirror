using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sequence.AspNetCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.PlayCard
{
    public sealed class PlayCardController : SequenceControllerBase
    {
        private readonly PlayCardHandler _handler;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public PlayCardController(
            PlayCardHandler handler,
            IMemoryCache cache,
            ILogger<PlayCardController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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

        [HttpPost("/games/{id:guid}/dead-card")]
        [ExchangeDeadCardFailedExceptionFilter]
        public async Task<ActionResult<GameEvent>> Post(
            [FromRoute] Guid id,
            [FromBody] ExchangeDeadCardForm form,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var deadCard = form.DeadCard;
            var gameEvent = await _handler.ExchangeDeadCardAsync(gameId, Player, deadCard, cancellationToken);
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

    public sealed class ExchangeDeadCardForm
    {
        [Required]
        public Card DeadCard { get; set; }
    }
}
