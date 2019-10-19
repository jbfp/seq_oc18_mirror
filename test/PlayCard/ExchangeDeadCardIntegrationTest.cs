using Microsoft.AspNetCore.Mvc.Testing;
using Sequence.Test.Postgres;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Test.PlayCard
{
    [Trait("Category", "Integration")]
    public sealed class ExchangeDeadCardIntegrationTest : IntegrationTestBase
    {
        public const string BasePath = "/games/{0}/dead-card";

        public ExchangeDeadCardIntegrationTest(
            PostgresDockerContainerFixture fixture,
            WebApplicationFactory<Startup> factory) : base(fixture, factory)
        {
        }

        [Fact]
        public async Task RouteIsProtected()
        {
            var path = string.Format(CultureInfo.InvariantCulture, BasePath, Guid.NewGuid());
            var requestUri = new Uri(path, UriKind.Relative);
            var body = new object();

            using var response = await UnauthorizedClient.PostAsJsonAsync(requestUri, body);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task InvalidFormBadRequest()
        {
            var path = string.Format(CultureInfo.InvariantCulture, BasePath, Guid.NewGuid());
            var requestUri = new Uri(path, UriKind.Relative);
            var body = new { deadCard = (object?)null };

            using var response = await AuthorizedClient.PostAsJsonAsync(requestUri, body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task NotFound()
        {
            var path = string.Format(CultureInfo.InvariantCulture, BasePath, Guid.NewGuid());
            var requestUri = new Uri(path, UriKind.Relative);
            var body = new { deadCard = new { deckNo = "one", suit = "spades", rank = "ace" } };

            using var response = await AuthorizedClient.PostAsJsonAsync(requestUri, body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
