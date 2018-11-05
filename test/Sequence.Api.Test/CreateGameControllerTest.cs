using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class CreateGameControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new CreateGameController(handler: null)
            );
        }
    }
}
