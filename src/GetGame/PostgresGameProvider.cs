using Sequence.Postgres;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGame
{
    public sealed class PostgresGameProvider : IGameProvider
    {
        private readonly Postgres.PostgresGameProvider _gameProvider;

        public PostgresGameProvider(Postgres.PostgresGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public async Task<GameState> GetGameStateByIdAsync(
            GameId gameId,
            CancellationToken cancellationToken)
        {
            var tuple = await _gameProvider.GetById(gameId, cancellationToken);

            if (tuple == null)
            {
                return null;
            }

            var initialState = new Sequence.GameState(tuple.Item1);
            var gameEvents = tuple.Item2;
            return new GameState(initialState, gameEvents);
        }
    }
}
