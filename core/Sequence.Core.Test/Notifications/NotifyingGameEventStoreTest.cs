using Moq;
using Sequence.Core.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Core.Test.Notifications
{
    public sealed class NotifyingGameEventStoreTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            var store = Mock.Of<IGameEventStore>();
            var notifier = Mock.Of<IGameUpdatedNotifier>();

            Assert.Throws<ArgumentNullException>(
                paramName: "store",
                () => new NotifyingGameEventStore(store: null, notifier)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "notifier",
                () => new NotifyingGameEventStore(store, notifier: null)
            );
        }

        private readonly Mock<IGameEventStore> _store = new Mock<IGameEventStore>();
        private readonly Mock<IGameUpdatedNotifier> _notifier = new Mock<IGameUpdatedNotifier>();
        private readonly NotifyingGameEventStore _sut;

        private readonly GameId _gameId = GameIdGenerator.Generate();
        private readonly GameEvent _gameEvent = new GameEvent();

        public NotifyingGameEventStoreTest()
        {
            _store
                .Setup(s => s.AddEventAsync(_gameId, It.IsAny<GameEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _notifier
                .Setup(n => n.SendAsync(_gameId, It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _sut = new NotifyingGameEventStore(_store.Object, _notifier.Object);
        }

        [Fact]
        public async Task DecoratesGameEventStore()
        {
            await _sut.AddEventAsync(_gameId, _gameEvent, CancellationToken.None);
            _store.Verify();
        }

        [Fact]
        public async Task NotifiesOtherPlayers()
        {
            await _sut.AddEventAsync(_gameId, _gameEvent, CancellationToken.None);
            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Call to notifier is an un-awaited Task.
            _notifier.Verify();
        }
    }
}
