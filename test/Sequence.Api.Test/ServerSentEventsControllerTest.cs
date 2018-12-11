using Microsoft.Extensions.Logging;
using Moq;
using Sequence.Core;
using Sequence.Core.Notifications;
using System;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class ServerSentEventsControllerTest
    {
        [Fact]
        public void Constructor_ThrowsIfArgsAreNull()
        {
            var handler = new SubscriptionHandler();
            var logger = Mock.Of<ILogger<ServerSentEventsController>>();

            Assert.Throws<ArgumentNullException>(
                paramName: "handler",
                testCode: () => new ServerSentEventsController(handler: null, logger)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "logger",
                testCode: () => new ServerSentEventsController(handler, logger: null)
            );
        }
    }
}
