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

        protected Task<NpgsqlConnectionFactory> CreateDatabaseAsync()
        {
            return _fixture.CreateDatabaseAsync(CancellationToken.None);
        }

        protected async Task<GameId> CreateGameAsync(
            NpgsqlConnectionFactory connectionFactory,
            NewPlayer player1 = null,
            NewPlayer player2 = null)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            var gameStore = new PostgresGameStore(connectionFactory);

            var newGame = new NewGame(
                players: new PlayerList(
                    player1 ?? new NewPlayer(Player1, PlayerType.User),
                    player2 ?? new NewPlayer(Player2, PlayerType.User)),
                seed: new Seed(42));

            return await gameStore.PersistNewGameAsync(newGame, CancellationToken.None);
        }

        protected async Task AddEventAsync(
            NpgsqlConnectionFactory connectionFactory,
            GameId gameId,
            GameEvent gameEvent)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            var gameEventStore = new PostgresGameEventStore(connectionFactory);

            await gameEventStore.AddEventAsync(gameId, gameEvent, CancellationToken.None);
        }
    }
}
