using Microsoft.AspNetCore.Mvc;
using Sequence.Core;
using Sequence.Core.GetGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class GetGamesController : SequenceControllerBase
    {
        private readonly GetGamesHandler _handler;

        public GetGamesController(GetGamesHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [HttpGet("/games")]
        public async Task<ActionResult<GetGamesResult>> Get(CancellationToken cancellationToken)
        {
            var gameIds = await _handler.GetGamesForPlayerAsync(PlayerId, cancellationToken);
            var result = new GetGamesResult(gameIds);
            return Ok(result);
        }
    }

    public sealed class GetGamesResult
    {
        public GetGamesResult(IReadOnlyCollection<GameId> gameIds)
        {
            GameIds = gameIds ?? throw new ArgumentNullException(nameof(gameIds));
        }

        public IReadOnlyCollection<GameId> GameIds { get; }
    }
}
