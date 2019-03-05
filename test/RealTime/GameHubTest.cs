using Sequence.RealTime;
using System;
using Xunit;

namespace Sequence.Test.RealTime
{
    public sealed class GameHubTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new GameHub(logger: null)
            );
        }
    }
}
