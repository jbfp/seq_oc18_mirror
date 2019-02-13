using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sequence.Api
{
    [ApiController]
    public sealed class LogsController : ControllerBase
    {
        private readonly ILogger _logger;

        public LogsController(ILogger<LogsController> logger)
        {
            _logger = logger;
        }

        [HttpPost("/logs")]
        public void Post([FromBody]LogModel model)
        {
            _logger.LogError("Client side error: {@Error}", model);
        }
    }

    public sealed class LogModel
    {
        public int LineNumber { get; set; }
        public string FileName { get; set; }
        public int ColumnNumber { get; set; }
        public string Stack { get; set; }
        public string Message { get; set; }
    }
}
