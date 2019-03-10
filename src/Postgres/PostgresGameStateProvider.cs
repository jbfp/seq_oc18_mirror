using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresGameStateProvider : IGameStateProvider
    {
        private readonly PostgresGameProvider _gameProvider;

        public PostgresGameStateProvider(PostgresGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public async Task<GameState> GetGameByIdAsync(
            GameId gameId,
            CancellationToken cancellationToken)
        {
            var tuple = await _gameProvider.GetById(gameId, cancellationToken);

            if (tuple == null)
            {
                return null;
            }

            return new GameState(tuple.Item1, tuple.Item2);
        }
    }
}
