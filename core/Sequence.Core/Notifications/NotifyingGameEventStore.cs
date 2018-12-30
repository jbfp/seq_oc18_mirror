using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.Notifications
{
    public sealed class NotifyingGameEventStore : IGameEventStore
    {
        private readonly IGameEventStore _store;
        private readonly IGameUpdatedNotifier _notifier;

        public NotifyingGameEventStore(IGameEventStore store, IGameUpdatedNotifier notifier)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        public async Task AddEventAsync(
            GameId gameId,
            GameEvent gameEvent,
            CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _store.AddEventAsync(gameId, gameEvent, cancellationToken);

            // Note: The task is stored in a variable to get rid of the 'un-awaited task' warning.
            Task _ = Task.Run(() => _notifier.SendAsync(gameId, version: gameEvent.Index));
        }
    }
}
