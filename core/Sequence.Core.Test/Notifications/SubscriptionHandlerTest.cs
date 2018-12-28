using Moq;
using Sequence.Core;
using Sequence.Core.Test;
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
        private readonly GameId _gameIdDummy = GameIdGenerator.Generate();

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
            var subscriber = Mock.Of<ISubscriber>();

            Assert.Throws<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.Subscribe(gameId: null, subscriber)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "subscriber",
                testCode: () => _sut.Subscribe(_gameIdDummy, subscriber: null)
            );
        }

        [Fact]
        public async Task SubscriberReceivesEvent()
        {
            // Given:
            var version = 0;
            var subscriber = new Mock<ISubscriber>();

            subscriber
                .Setup(s => s.UpdateGameAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut.Subscribe(_gameIdDummy, subscriber.Object);

            // When:
            await _sut.SendAsync(_gameIdDummy, version);

            // Then:
            subscriber.Verify(s => s.UpdateGameAsync(version), Times.Once);
        }

        [Fact]
        public async Task Unsubscribe()
        {
            // Given:
            var version = 0;
            var subscriber = new Mock<ISubscriber>();

            subscriber
                .Setup(s => s.UpdateGameAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut.Subscribe(_gameIdDummy, subscriber.Object)
                .Dispose(); // Unsubscribe.

            // When:
            await _sut.SendAsync(_gameIdDummy, version);

            // Then:
            subscriber.Verify(s => s.UpdateGameAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task MultipleSubscribers()
        {
            // Given:
            var version = 0;
            var subscribers = new List<Mock<ISubscriber>>(10);

            for (var i = 0; i < subscribers.Capacity; i++)
            {
                var subscriber = new Mock<ISubscriber>();

                subscriber
                    .Setup(s => s.UpdateGameAsync(It.IsAny<int>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                _sut.Subscribe(_gameIdDummy, subscriber.Object);

                subscribers.Add(subscriber);
            }

            // When:
            await _sut.SendAsync(_gameIdDummy, version);

            // Then:
            foreach (var subscriber in subscribers)
            {
                subscriber.Verify(s => s.UpdateGameAsync(version), Times.Once);
            }
        }
    }
}
