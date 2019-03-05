using Microsoft.Extensions.Logging;
using Moq;
using Sequence.GetGameList;
using System;
using Xunit;

namespace Sequence.Test.GetGameList
{
    public sealed class GetGameListControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new GetGameListHandler(Mock.Of<IGameListProvider>());
            var logger = Mock.Of<ILogger<GetGameListController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new GetGameListController(handler: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new GetGameListController(handler, logger: null)
            );
        }
    }
}
