using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Sequence.ClientSideLogging
{
    [ApiController]
    public sealed class ClientSideLoggingController : ControllerBase
    {
        private readonly ILogger _logger;

        public ClientSideLoggingController(ILogger<ClientSideLoggingController> logger)
        {
            _logger = logger;
        }

        [HttpPost("/logs")]
        public void Post([FromBody] LogModel model)
        {
            _logger.LogError("Client side error: {@Error}", model);
        }
    }

    public sealed class LogModel
    {
        public int? LineNumber { get; set; }
        public string? FileName { get; set; }
        public int? ColumnNumber { get; set; }
        public string? Stack { get; set; }
        public string? Message { get; set; }
    }
}
