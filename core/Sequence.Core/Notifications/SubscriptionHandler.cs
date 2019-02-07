using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sequence.Core.Notifications
{
    public interface ISubscriber
    {
        Task UpdateGameAsync(GameEvent gameEvent);
    }

    public sealed class SubscriptionHandler : IGameUpdatedNotifier
    {
        private readonly object _sync;
        private readonly Dictionary<GameId, List<ISubscriber>> _subscriptions;
        private readonly ILogger _logger;

        public SubscriptionHandler(ILogger<SubscriptionHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sync = new object();
            _subscriptions = new Dictionary<GameId, List<ISubscriber>>();
        }

        public async Task SendAsync(GameId gameId, GameEvent gameEvent)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            Task whenAll = Task.CompletedTask;

            lock (_sync)
            {
                if (_subscriptions.TryGetValue(gameId, out var subscribers))
                {
                    _logger.LogInformation(
                        "Sending update to {NumSubscribers} for {GameId}",
                        subscribers.Count, gameId);

                    whenAll = Task.WhenAll(subscribers.Select(
                        subscriber => NotifySubscriberAsync(subscriber, gameEvent)));
                }
                else
                {
                    _logger.LogInformation("No subscribers for {GameId}", gameId);
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
                _logger.LogInformation("User subscribing to {GameId}", gameId);

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
                _logger.LogInformation("User unsubscribing from {GameId}", gameId);

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

        private async Task NotifySubscriberAsync(ISubscriber subscriber, GameEvent gameEvent)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            try
            {
                await subscriber.UpdateGameAsync(gameEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred trying to send game update to client.");
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
