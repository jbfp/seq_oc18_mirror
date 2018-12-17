using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public GetGamesController(GetGamesHandler handler, ILogger<GetGamesController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/games")]
        public async Task<ActionResult<GameList>> Get(CancellationToken cancellationToken)
        {
            return await _handler.GetGamesForPlayerAsync(PlayerId, cancellationToken);
        }
    }
}
