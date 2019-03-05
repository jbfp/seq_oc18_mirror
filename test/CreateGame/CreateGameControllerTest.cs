using Microsoft.Extensions.Logging;
using Moq;
using Sequence.CreateGame;
using System;
using Xunit;

namespace Sequence.Test.CreateGame
{
    public sealed class CreateGameControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new CreateGameHandler(Mock.Of<IRandomFactory>(), Mock.Of<IGameStore>());
            var logger = Mock.Of<ILogger<CreateGameController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new CreateGameController(handler: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new CreateGameController(handler, logger: null)
            );
        }
    }
}
