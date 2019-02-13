using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Api.Caching;
using Sequence.Core;
using System;
using Xunit;

namespace Sequence.Api.Test.Caching
{
    public sealed class CachedGameStoreTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var gameProvider = Mock.Of<IGameProvider>();
            var cache = Mock.Of<IMemoryCache>();
            var logger = Mock.Of<ILogger<CachedGameStore>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "gameProvider",
                testCode: () => new CachedGameStore(gameProvider: null, cache, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "cache",
                testCode: () => new CachedGameStore(gameProvider, cache: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new CachedGameStore(gameProvider, cache, logger: null)
            );
        }
    }
}
