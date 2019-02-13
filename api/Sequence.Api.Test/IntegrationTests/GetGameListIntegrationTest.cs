using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Sequence.Api.Test.IntegrationTests;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Api.Test.IntegrationTests
{
    public sealed class GetGameListIntegrationTest : IntegrationTestBase
    {
        private const string BasePath = "/games";

        public GetGameListIntegrationTest(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task RoutesAreProtected()
        {
            using (var response = await UnauthorizedClient.GetAsync(BasePath))
            {
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }


        [Fact]
        public async Task GetGamesReturnsOk()
        {
            using (var response = await AuthorizedClient.GetAsync("/games"))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetGamesReturnsGames()
        {
            // Given:
            await CreateGameAsync();

            // When:
            var result = JObject.Parse(await AuthorizedClient.GetStringAsync("/games"));

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
