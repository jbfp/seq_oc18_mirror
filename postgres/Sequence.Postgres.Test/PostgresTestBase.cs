using Microsoft.Extensions.Options;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres.Test
{
    public abstract class PostgresTestBase
    {
        private readonly PostgresDockerContainerFixture _fixture;

        protected PostgresTestBase(PostgresDockerContainerFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        public PlayerHandle Player1 { get; } = new PlayerHandle("Player 1");
        public PlayerHandle Player2 { get; } = new PlayerHandle("Player 2");

        protected Task<IOptions<PostgresOptions>> CreateDatabaseAsync()
        {
            return _fixture.CreateDatabaseAsync(CancellationToken.None);
        }

        protected async Task<GameId> CreateGameAsync(IOptions<PostgresOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var gameStore = new PostgresGameStore(options);

            var newGame = new NewGame(
                players: new PlayerList(
                    new NewPlayer(Player1, PlayerType.User),
                    new NewPlayer(Player2, PlayerType.User)),
                seed: new Seed(42));

            return await gameStore.PersistNewGameAsync(newGame, CancellationToken.None);
        }

        protected async Task AddEventAsync(IOptions<PostgresOptions> options, GameId gameId, GameEvent gameEvent)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            var gameEventStore = new PostgresGameEventStore(options);

            await gameEventStore.AddEventAsync(gameId, gameEvent, CancellationToken.None);
        }
    }
}
