using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Api.Caching;
using Sequence.Core;
using System;
using Xunit;

namespace Sequence.Api.Test.Caching
{
    public sealed class CachedGameEventStoreTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var gameEventStore = Mock.Of<IGameEventStore>();
            var cache = Mock.Of<IMemoryCache>();
            var logger = Mock.Of<ILogger<CachedGameEventStore>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "gameEventStore",
                testCode: () => new CachedGameEventStore(gameEventStore: null, cache, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "cache",
                testCode: () => new CachedGameEventStore(gameEventStore, cache: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new CachedGameEventStore(gameEventStore, cache, logger: null)
            );
        }
    }
}
