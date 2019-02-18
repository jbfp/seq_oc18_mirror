using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api.Caching
{
    public sealed class CachedGameEventStore : IGameEventStore
    {
        private readonly IGameEventStore _gameEventStore;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public CachedGameEventStore(
            IGameEventStore gameEventStore,
            IMemoryCache cache,
            ILogger<CachedGameEventStore> logger)
        {
            _gameEventStore = gameEventStore ?? throw new ArgumentNullException(nameof(gameEventStore));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddEventAsync(GameId gameId, GameEvent gameEvent, CancellationToken cancellationToken)
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

            // Remove game from cache and force reload from database.
            _logger.LogInformation("Evicting game {GameId} from cache because of new game event.", gameId);
            _cache.Remove(gameId);

            await _gameEventStore.AddEventAsync(gameId, gameEvent, cancellationToken);
        }
    }
}
