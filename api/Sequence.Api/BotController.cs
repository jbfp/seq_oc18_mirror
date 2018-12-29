using Microsoft.AspNetCore.Mvc;
using Sequence.Core;
using System;
using System.Linq;
using System.Threading;

namespace Sequence.Api
{
    public sealed class BotController : SequenceControllerBase
    {
        [HttpGet("/bots")]
        public ActionResult<BotListResult> Get(CancellationToken cancellationToken)
        {
            return new BotListResult
            {
                BotTypes = BotProvider.BotTypes.Keys.ToArray()
            };
        }
    }

    public sealed class BotListResult
    {
        public string[] BotTypes { get; set; }
    }
}
