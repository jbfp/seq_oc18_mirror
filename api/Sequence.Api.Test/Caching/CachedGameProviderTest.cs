using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Api.Caching;
using Sequence.Core;
using System;
using Xunit;

namespace Sequence.Api.Test.Caching
{
    public sealed class CachedGameProviderTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var gameProvider = Mock.Of<IGameProvider>();
            var cache = Mock.Of<IMemoryCache>();
            var logger = Mock.Of<ILogger<CachedGameProvider>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "gameProvider",
                testCode: () => new CachedGameProvider(gameProvider: null, cache, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "cache",
                testCode: () => new CachedGameProvider(gameProvider, cache: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new CachedGameProvider(gameProvider, cache, logger: null)
            );
        }
    }
}
