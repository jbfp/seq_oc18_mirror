using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class RandomSeedProviderTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new RandomSeedProvider(logger: null)
            );
        }
    }
}
