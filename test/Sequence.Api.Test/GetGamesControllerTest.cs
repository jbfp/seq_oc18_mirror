using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class GetGamesControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new GetGamesController(handler: null)
            );
        }
    }
}
