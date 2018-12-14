using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Core.GetGames;
using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class GetGamesControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new GetGamesHandler(Mock.Of<IGameListProvider>());
            var logger = Mock.Of<ILogger<GetGamesController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new GetGamesController(handler: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new GetGamesController(handler, logger: null)
            );
        }
    }
}
