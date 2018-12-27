using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Core;
using Sequence.Core.Play;
using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class PlayControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new PlayHandler(
                Mock.Of<IGameProvider>(),
                Mock.Of<IGameEventStore>(),
                Mock.Of<IGameUpdatedNotifier>());

            var logger = Mock.Of<ILogger<PlayController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new PlayController(handler: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new PlayController(handler, logger: null)
            );
        }
    }
}
