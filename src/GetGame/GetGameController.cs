using Microsoft.AspNetCore.Mvc;
using Sequence.AspNetCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGame
{
    public sealed class GetGameController : SequenceControllerBase
    {
        private const int BoardCacheTime = 10519200; // seconds.

        private readonly IGameProvider _provider;

        public GetGameController(IGameProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
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
