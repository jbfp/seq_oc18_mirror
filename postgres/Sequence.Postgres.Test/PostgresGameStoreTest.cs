using Sequence.Core;
using Sequence.Core.CreateGame;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Postgres.Test
{
    [Collection(PostgresDockerContainerCollection.Name)]
    public sealed class PostgresGameStoreTest : PostgresTestBase
    {
        public PostgresGameStoreTest(PostgresDockerContainerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CanPersistNewGame()
        {
            // Given:
            var options = await CreateDatabaseAsync();
            var sut = new PostgresGameStore(options);
            var newGame = new NewGame(
                players: new PlayerList(
                    new PlayerId("player 1"),
                    new PlayerId("player 2")),
                seed: new Seed(42));

            // When:
            var gameId = await sut.PersistNewGameAsync(newGame, CancellationToken.None);

            // Then:
            Assert.NotNull(gameId);
        }
    }
}
