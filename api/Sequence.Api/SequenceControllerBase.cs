using Microsoft.AspNetCore.Mvc;
using Sequence.Core;
using System;

namespace Sequence.Api
{
    [ApiController]
    [CustomAuthorize]
    [GameNotFoundExceptionFilter]
    public abstract class SequenceControllerBase : ControllerBase
    {
        protected PlayerId PlayerId
        {
            get
            {
                if (Request.Headers.TryGetValue("Authorization", out var values) && values.Count > 0)
                {
                    return new PlayerId(string.Join(' ', values.ToArray()));
                }

                throw new InvalidOperationException("No authorization header.");
            }
        }
    }
}
