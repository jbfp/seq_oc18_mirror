using Sequence.ClientSideLogging;
using System;
using Xunit;

namespace Sequence.Test.ClientSideLogging
{
    public sealed class ClientSideLoggingControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new ClientSideLoggingController(logger: null)
            );
        }
    }
}
