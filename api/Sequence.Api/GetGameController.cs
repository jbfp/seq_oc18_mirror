using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public GetGameController(
            GetGameHandler handler,
            IMemoryCache cache,
            ILogger<GetGameController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/games/{id:guid}")]
        public async Task<ActionResult<GetGameResult>> Get(
            [FromRoute] Guid id,
            [FromQuery] int? version,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var cacheKey = string.Format(CacheKeys.GameVersionKey, gameId, Player);

            if (_cache.TryGetValue<GameView>(cacheKey, out var view))
            {
                if (version.HasValue && version.Equals(view.Index))
                {
                    return StatusCode(304);
                }
                else
                {
                    return Ok(new GetGameResult(view));
                }
            }
            else
            {
                view = await _handler.GetGameViewForPlayerAsync(gameId, Player, cancellationToken);
                _cache.Set(cacheKey, view);
                return Ok(new GetGameResult(view));
            }
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
