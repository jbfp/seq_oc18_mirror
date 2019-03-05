using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sequence.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameList
{
    public sealed class GetGameListController : SequenceControllerBase
    {
        private readonly GetGameListHandler _handler;
        private readonly ILogger _logger;

        public GetGameListController(GetGameListHandler handler, ILogger<GetGameListController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/games")]
        public async Task<ActionResult<GameList>> Get(CancellationToken cancellationToken)
        {
            return await _handler.GetGamesForPlayerAsync(Player, cancellationToken);
        }
    }
}
