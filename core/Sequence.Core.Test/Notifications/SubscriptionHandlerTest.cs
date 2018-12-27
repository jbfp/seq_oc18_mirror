using Moq;
using Sequence.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Core.Notifications.Test
{
    public sealed class SubscriptionHandlerTest
    {
        private readonly SubscriptionHandler _sut = new SubscriptionHandler();

        [Fact]
        public async Task SendAsync_NullArguments()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.SendAsync(gameId: null, version: 0)
            );
        }

        [Fact]
        public void Subscribe_NullArguments()
        {
            var gameId = new GameId("dummy");
            var subscriber = Mock.Of<ISubscriber>();

            Assert.Throws<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.Subscribe(gameId: null, subscriber)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "subscriber",
                testCode: () => _sut.Subscribe(gameId, subscriber: null)
            );
        }

        [Fact]
        public async Task SubscriberReceivesEvent()
        {
            // Given:
            var gameId = new GameId("dummy");
            var version = 0;
            var subscriber = new Mock<ISubscriber>();

            subscriber
                .Setup(s => s.UpdateGameAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut.Subscribe(gameId, subscriber.Object);

            // When:
            await _sut.SendAsync(gameId, version);

            // Then:
            subscriber.Verify(s => s.UpdateGameAsync(version), Times.Once);
        }

        [Fact]
        public async Task Unsubscribe()
        {
            // Given:
            var gameId = new GameId("dummy");
            var version = 0;
            var subscriber = new Mock<ISubscriber>();

            subscriber
                .Setup(s => s.UpdateGameAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut.Subscribe(gameId, subscriber.Object)
                .Dispose(); // Unsubscribe.

            // When:
            await _sut.SendAsync(gameId, version);

            // Then:
            subscriber.Verify(s => s.UpdateGameAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task MultipleSubscribers()
        {
            // Given:
            var gameId = new GameId("dummy");
            var version = 0;
            var subscribers = new List<Mock<ISubscriber>>(10);

            for (var i = 0; i < subscribers.Capacity; i++)
            {
                var subscriber = new Mock<ISubscriber>();

                subscriber
                    .Setup(s => s.UpdateGameAsync(It.IsAny<int>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                _sut.Subscribe(gameId, subscriber.Object);

                subscribers.Add(subscriber);
            }

            // When:
            await _sut.SendAsync(gameId, version);

            // Then:
            foreach (var subscriber in subscribers)
            {
                subscriber.Verify(s => s.UpdateGameAsync(version), Times.Once);
            }
        }
    }
}
