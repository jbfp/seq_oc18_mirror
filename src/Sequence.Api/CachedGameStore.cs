using Sequence.Core;
using Sequence.Core.Play;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class CachedGameStore : IGameProvider, IGameEventStore
    {
        private readonly IGameProvider _gameProvider;
        private readonly IGameEventStore _gameEventStore;

        private readonly Dictionary<GameId, Game> _cache = new Dictionary<GameId, Game>();

        public CachedGameStore(IGameProvider gameProvider, IGameEventStore gameEventStore)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameEventStore = gameEventStore ?? throw new ArgumentNullException(nameof(gameEventStore));
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

            await _gameEventStore.AddEventAsync(gameId, gameEvent, cancellationToken);
            Game game = await GetGameByIdAsync(gameId, cancellationToken);
            game.Apply(gameEvent);
        }

        public async Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (_cache.TryGetValue(gameId, out Game game))
            {
                return game;
            }

            game = await _gameProvider.GetGameByIdAsync(gameId, cancellationToken);

            if (game != null)
            {
                _cache[gameId] = game;
            }

            return game;
        }
    }
}
