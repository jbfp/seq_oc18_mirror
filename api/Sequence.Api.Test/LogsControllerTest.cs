using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class LogsControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new LogsController(logger: null)
            );
        }
    }
}
