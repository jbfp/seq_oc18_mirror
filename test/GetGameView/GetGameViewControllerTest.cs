using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Sequence.GetGameView;
using System;
using Xunit;

namespace Sequence.Test.GetGameView
{
    public sealed class GetGameViewControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new GetGameViewHandler(Mock.Of<IGameStateProvider>());
            var cache = Mock.Of<IMemoryCache>();
            var logger = Mock.Of<ILogger<GetGameViewController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new GetGameViewController(handler: null, cache, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "cache",
                testCode: () => new GetGameViewController(handler, cache: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new GetGameViewController(handler, cache, logger: null)
            );
        }
    }
}
