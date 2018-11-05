using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class PlayControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new PlayController(handler: null)
            );
        }
    }
}
