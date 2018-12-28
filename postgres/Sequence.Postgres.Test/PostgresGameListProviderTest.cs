using Sequence.Core;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Postgres.Test
{
    [Collection(PostgresDockerContainerCollection.Name)]
    public class PostgresGameListProviderTest : PostgresTestBase
    {
        public PostgresGameListProviderTest(PostgresDockerContainerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CanGetGameList()
        {
            // Given:
            var options = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(options);
            var player = new PlayerHandle("player 1");
            var opponents = new[] { new PlayerHandle("player 2") };
            var sut = new PostgresGameListProvider(options);

            // When:
            var gameList = await sut.GetGamesForPlayerAsync(player, CancellationToken.None);

            // Then:
            Assert.NotNull(gameList);
            var gameListItem = Assert.Single(gameList.Games);
            Assert.Equal(gameId, gameListItem.GameId);
            Assert.Equal(player, gameListItem.CurrentPlayer);
            Assert.Equal(opponents, gameListItem.Opponents);
        }

        [Fact]
        public async Task GameList_NextPlayerIdIsNullWhenGameIsOver()
        {
            // Given:
            var options = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(options);
            var player = new PlayerHandle("player 1");

            await AddEventAsync(options, gameId, new GameEvent
            {
                ByPlayerId = new PlayerId(1),
                CardUsed = new Card(DeckNo.One, Suit.Spades, Rank.Ace),
                Chip = Team.Red,
                Coord = new Coord(4, 2),
                Index = 1,
                NextPlayerId = null
            });

            var sut = new PostgresGameListProvider(options);

            // When:
            var gameList = await sut.GetGamesForPlayerAsync(player, CancellationToken.None);

            // Then:
            var gameListItem = Assert.Single(gameList.Games);
            Assert.Null(gameListItem.CurrentPlayer);
        }
    }
}
