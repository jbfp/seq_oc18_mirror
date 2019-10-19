using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Sequence.Test.Postgres;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Test.GetGameList
{
    [Trait("Category", "Integration")]
    public sealed class GetGameListIntegrationTest : IntegrationTestBase
    {
        private static readonly Uri BasePath = new Uri("/games", UriKind.Relative);

        public GetGameListIntegrationTest(
            PostgresDockerContainerFixture fixture,
            WebApplicationFactory<Startup> factory)
            : base(fixture, factory)
        {
        }

        [Fact]
        public async Task RoutesAreProtected()
        {
            using var response = await UnauthorizedClient.GetAsync(BasePath);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [Fact]
        public async Task GetGamesReturnsOk()
        {
            using var response = await AuthorizedClient.GetAsync(BasePath);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetGamesReturnsGames()
        {
            // Given:
            await CreateGameAsync();

            // When:
            var result = JObject.Parse(await AuthorizedClient.GetStringAsync(BasePath));

            // Then:
            var games = result["games"].ToObject<JArray>();
            Assert.NotEmpty(games);
            var game = games[0].ToObject<JObject>();
            Assert.NotNull(game["gameId"]);
            Assert.Equal("test_player", game["currentPlayer"].ToObject<string>());
            Assert.Equal(new[] { "test" }, game["opponents"].ToObject<string[]>());
        }
    }
}
