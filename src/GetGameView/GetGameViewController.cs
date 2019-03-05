using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sequence.AspNetCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameView
{
    public sealed class GetGameViewController : SequenceControllerBase
    {
        private const int BoardCacheTime = 10519200; // seconds.

        private readonly GetGameViewHandler _handler;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public GetGameViewController(
            GetGameViewHandler handler,
            IMemoryCache cache,
            ILogger<GetGameViewController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/boards")]
        public ActionResult<string[]> Get()
        {
            var enumType = typeof(BoardType);
            var boardTypes = Enum.GetNames(enumType);
            return boardTypes;
        }

        [HttpGet("/boards/{boardType}")]
        [ResponseCache(Duration = BoardCacheTime)]
        public ActionResult Get([Enum(typeof(BoardType))]BoardType boardType)
        {
            IBoardType boardTypeInstance;

            try
            {
                boardTypeInstance = boardType.Create();
            }
            catch (ArgumentOutOfRangeException)
            {
                return ValidationProblem(ModelState);
            }

            return Ok(boardTypeInstance.Board);
        }

        [HttpGet("/games/{id:guid}")]
        public async Task<ActionResult<GetGameResult>> Get(
            [FromRoute] Guid id,
            [FromQuery] int? version,
            CancellationToken cancellationToken)
        {
            var gameId = new GameId(id);
            var view = await _handler.GetGameViewForPlayerAsync(gameId, Player, cancellationToken);
            return Ok(new GetGameResult(view));
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
