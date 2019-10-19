using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGame
{
    public sealed class PostgresGameProvider : IGameProvider
    {
        private readonly Postgres.PostgresGameProvider _gameProvider;

        public PostgresGameProvider(Postgres.PostgresGameProvider gameProvider)
        {
            _gameProvider = gameProvider;
        }

        public async Task<Game?> GetGameByIdAsync(
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
            return new Game(initialState, gameEvents);
        }
    }
}
