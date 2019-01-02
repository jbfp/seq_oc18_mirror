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
        private readonly GameEvent _gameEventDummy = new GameEvent();

        [Fact]
        public async Task SendAsync_NullArguments()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "gameId",
                testCode: () => _sut.SendAsync(gameId: null, _gameEventDummy)
            );

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "gameEvent",
                testCode: () => _sut.SendAsync(_gameIdDummy, gameEvent: null)
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
            var subscriber = new Mock<ISubscriber>();

            subscriber
                .Setup(s => s.UpdateGameAsync(It.IsAny<GameEvent>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut.Subscribe(_gameIdDummy, subscriber.Object);

            // When:
            await _sut.SendAsync(_gameIdDummy, _gameEventDummy);

            // Then:
            subscriber.Verify(s => s.UpdateGameAsync(_gameEventDummy), Times.Once);
        }

        [Fact]
        public async Task Unsubscribe()
        {
            // Given:
            var subscriber = new Mock<ISubscriber>();

            subscriber
                .Setup(s => s.UpdateGameAsync(It.IsAny<GameEvent>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut.Subscribe(_gameIdDummy, subscriber.Object)
                .Dispose(); // Unsubscribe.

            // When:
            await _sut.SendAsync(_gameIdDummy, _gameEventDummy);

            // Then:
            subscriber.Verify(s => s.UpdateGameAsync(It.IsAny<GameEvent>()), Times.Never);
        }

        [Fact]
        public async Task MultipleSubscribers()
        {
            // Given:
            var subscribers = new List<Mock<ISubscriber>>(10);

            for (var i = 0; i < subscribers.Capacity; i++)
            {
                var subscriber = new Mock<ISubscriber>();

                subscriber
                    .Setup(s => s.UpdateGameAsync(It.IsAny<GameEvent>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                _sut.Subscribe(_gameIdDummy, subscriber.Object);

                subscribers.Add(subscriber);
            }

            // When:
            await _sut.SendAsync(_gameIdDummy, _gameEventDummy);

            // Then:
            foreach (var subscriber in subscribers)
            {
                subscriber.Verify(s => s.UpdateGameAsync(_gameEventDummy), Times.Once);
            }
        }
    }
}
