using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sequence.PlayCard;

namespace Sequence.AspNetCore
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

    internal sealed class ExchangeDeadCardFailedExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ExchangeDeadCardFailedException ex)
            {
                context.Result = new OkObjectResult(new { ex.Error });
            }
        }
    }
}
