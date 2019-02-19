using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Api.Test.IntegrationTests
{
    public sealed class GetGameIntegrationTest : IntegrationTestBase
    {
        private const string BasePath = "/games";

        public GetGameIntegrationTest(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData("32ceba86-ff6a-46cc-8b79-838dbec3ccb7")]
        [InlineData("9f97262e-284f-49a3-87ab-22eb0e5b6289")]
        [InlineData("b460f19a-7261-4bf8-8b0b-1097df0bf611")]
        public async Task RoutesAreProtected(string gameId)
        {
            var requestUri = $"{BasePath}/{gameId}";

            using (var response = await UnauthorizedClient.GetAsync(requestUri))
            {
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetGameReturnsNotFound()
        {
            var requestUri = $"{BasePath}/idontexistforsure";

            using (var response = await AuthorizedClient.GetAsync(requestUri))
            {
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetGameReturnsOk()
        {
            var gamePath = await CreateGameAsync();

            using (var response = await AuthorizedClient.GetAsync(gamePath))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetGameReturnsNotModified()
        {
            var gamePath = await CreateGameAsync();

            using (var response = await AuthorizedClient.GetAsync(gamePath))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var gamePathWithVersion = $"{gamePath}?version={0}";

            using (var response = await AuthorizedClient.GetAsync(gamePathWithVersion))
            {
                Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            }
        }
    }
}
