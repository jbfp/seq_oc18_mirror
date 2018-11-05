using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class GetGameControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new GetGameController(handler: null)
            );
        }
    }
}
