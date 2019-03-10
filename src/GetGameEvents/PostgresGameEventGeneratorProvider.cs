using Sequence.Postgres;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameEvents
{
    public sealed class PostgresGameEventGeneratorProvider : IGameEventGeneratorProvider
    {
        private readonly PostgresGameProvider _gameProvider;

        public PostgresGameEventGeneratorProvider(PostgresGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public async Task<GameEventGenerator> GetGameEventGeneratorByIdAsync(
            GameId gameId,
            CancellationToken cancellationToken)
        {
            var tuple = await _gameProvider.GetById(gameId, cancellationToken);

            if (tuple == null)
            {
                return null;
            }

            return new GameEventGenerator(tuple.Item1, tuple.Item2);
        }
    }
}
