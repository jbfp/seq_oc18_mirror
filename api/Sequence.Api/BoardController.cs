using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Core;
using Sequence.Core.Boards;
using System;

namespace Sequence.Api
{
    public sealed class BoardController : SequenceControllerBase
    {
        private const int CacheTime = 10519200; // seconds.

        [HttpGet("/boards")]
        public IActionResult Get()
        {
            var enumType = typeof(BoardType);
            var boardTypes = Enum.GetNames(enumType);
            return Ok(boardTypes);
        }

        [HttpGet("/boards/{boardType}")]
        [ResponseCache(Duration = CacheTime)]
        public IActionResult Get([Enum(typeof(BoardType))]BoardType boardType)
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
    }
}
