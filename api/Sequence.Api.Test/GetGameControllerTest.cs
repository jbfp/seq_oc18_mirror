using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Core;
using Sequence.Core.GetGame;
using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class GetGameControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new GetGameHandler(Mock.Of<IGameProvider>());
            var logger = Mock.Of<ILogger<GetGameController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new GetGameController(handler: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new GetGameController(handler, logger: null)
            );
        }
    }
}
