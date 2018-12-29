using Sequence.Core;
using Sequence.Core.CreateGame;
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
            var opponents = new[] { Player2 };
            var sut = new PostgresGameListProvider(options);

            // When:
            var gameList = await sut.GetGamesForPlayerAsync(Player1, CancellationToken.None);

            // Then:
            Assert.NotNull(gameList);
            var gameListItem = Assert.Single(gameList.Games);
            Assert.Equal(gameId, gameListItem.GameId);
            Assert.Equal(Player1, gameListItem.CurrentPlayer);
            Assert.Equal(opponents, gameListItem.Opponents);
        }

        [Fact]
        public async Task GameList_NextPlayerIdIsNullWhenGameIsOver()
        {
            // Given:
            var options = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(options);

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
            var gameList = await sut.GetGamesForPlayerAsync(Player1, CancellationToken.None);

            // Then:
            var gameListItem = Assert.Single(gameList.Games);
            Assert.Null(gameListItem.CurrentPlayer);
        }

        [Fact]
        public async Task CannotGetGamesAsBot()
        {
            // Given:
            var options = await CreateDatabaseAsync();
            var player = new PlayerHandle("Super Bot");
            var gameId = await CreateGameAsync(options,
                player1: new NewPlayer(player, PlayerType.Bot));

            var sut = new PostgresGameListProvider(options);

            // When:
            var gameList = await sut.GetGamesForPlayerAsync(player, CancellationToken.None);

            // Then:
            Assert.Empty(gameList.Games);
        }
    }
}
