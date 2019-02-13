using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using Sequence.Core.Play;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api.Caching
{

    public sealed class CachedGameStore : IGameProvider
    {
        private readonly IGameProvider _gameProvider;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public CachedGameStore(
            IGameProvider gameProvider,
            IMemoryCache cache,
            ILogger<CachedGameStore> logger)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _cache.GetOrCreateAsync(gameId, entry =>
            {
                entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                {
                    EvictionCallback = LogEviction,
                    State = _logger,
                });

                _logger.LogInformation("Populating the cache for {GameId}", gameId);
                return _gameProvider.GetGameByIdAsync(gameId, cancellationToken);
            });
        }

        private static void LogEviction(object key, object value, EvictionReason reason, object state)
        {
            var logger = (ILogger)state;
            var message = "Cache entry for {GameId} was evicted because {Reason}";
            logger.LogInformation(message, key, reason);
        }
    }
}
