using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Api.Test.IntegrationTests
{
    public sealed class BotsIntegrationTest : IntegrationTestBase
    {
        private const string BasePath = "/bots";

        public BotsIntegrationTest(WebApplicationFactory<Startup> factory) : base(factory)
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
        public async Task GetBotsReturnsCorrectResult()
        {
            using (var response = await AuthorizedClient.GetAsync(BasePath))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                Assert.NotEmpty(json["botTypes"]);
            }
        }
    }
}
