using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Sequence.PlayCard;
using Sequence.RealTime;
using System;
using Xunit;

namespace Sequence.Test.PlayCard
{
    public sealed class PlayCardControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new PlayCardHandler(
                Mock.Of<IGameStateProvider>(),
                Mock.Of<IGameEventStore>(),
                Mock.Of<IHubContext<GameHub, IGameHubClient>>());
            var cache = Mock.Of<IMemoryCache>();
            var logger = Mock.Of<ILogger<PlayCardController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new PlayCardController(handler: null, cache, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "cache",
                testCode: () => new PlayCardController(handler, cache: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new PlayCardController(handler, cache, logger: null)
            );
        }
    }
}
