using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Core;
using Sequence.Core.Play;
using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class CachedGameStoreTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var gameProvider = Mock.Of<IGameProvider>();
            var gameEventStore = Mock.Of<IGameEventStore>();
            var logger = Mock.Of<ILogger<CachedGameStore>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "gameProvider",
                testCode: () => new CachedGameStore(gameProvider: null, gameEventStore, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "gameEventStore",
                testCode: () => new CachedGameStore(gameProvider, gameEventStore: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new CachedGameStore(gameProvider, gameEventStore, logger: null)
            );
        }
    }
}
