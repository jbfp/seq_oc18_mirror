using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sequence.Core;

namespace Sequence.Api
{
    internal sealed class GameNotFoundExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is GameNotFoundException)
            {
                context.Result = new NotFoundResult();
            }
        }
    }

    internal sealed class PlayCardFailedExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is PlayCardFailedException ex)
            {
                context.Result = new OkObjectResult(new { ex.Error });
            }
        }
    }
}
