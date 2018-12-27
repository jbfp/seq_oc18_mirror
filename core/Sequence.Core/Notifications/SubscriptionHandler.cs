using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sequence.Core.Notifications
{
    public interface ISubscriber
    {
        Task UpdateGameAsync(int version);
    }

    public sealed class SubscriptionHandler : IGameUpdatedNotifier
    {
        private readonly object _sync;
        private readonly Dictionary<GameId, List<ISubscriber>> _subscriptions;

        public SubscriptionHandler()
        {
            _sync = new object();
            _subscriptions = new Dictionary<GameId, List<ISubscriber>>();
        }

        public async Task SendAsync(GameId gameId, int version)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            Task whenAll = Task.CompletedTask;

            lock (_sync)
            {
                if (_subscriptions.TryGetValue(gameId, out var subscribers))
                {
                    whenAll = Task.WhenAll(subscribers.Select(
                        subscriber => NotifySubscriberAsync(subscriber, version)));
                }
            }

            await whenAll;
        }

        public IDisposable Subscribe(GameId gameId, ISubscriber subscriber)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            // TODO: Check that game exists.

            lock (_sync)
            {
                if (_subscriptions.TryGetValue(gameId, out var subscribers))
                {
                    subscribers.Add(subscriber);
                }
                else
                {
                    _subscriptions.Add(gameId, new List<ISubscriber> { subscriber });
                }
            }

            return new Subscription(unsubscribe: () => Unsubscribe(gameId, subscriber));
        }

        private void Unsubscribe(GameId gameId, ISubscriber subscriber)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            lock (_sync)
            {
                if (_subscriptions.TryGetValue(gameId, out var subscribers))
                {
                    subscribers.Remove(subscriber);

                    if (subscribers.Count == 0)
                    {
                        _subscriptions.Remove(gameId);
                    }
                }
            }
        }

        private async Task NotifySubscriberAsync(ISubscriber subscriber, int version)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            try
            {
                await subscriber.UpdateGameAsync(version);
            }
            catch
            {
                // TODO: Logging.
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
            }

            public void Dispose()
            {
                _unsubscribe();
            }
        }
    }
}
