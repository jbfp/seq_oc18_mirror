using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Api.Test.IntegrationTests
{
    public sealed class NotificationsIntegrationTest : IntegrationTestBase
    {
        private readonly SeedProviderStub _seedProvider = new SeedProviderStub();

        public NotificationsIntegrationTest(WebApplicationFactory<Startup> factory) : base(factory)
        {
            Factory = Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<ISeedProvider>(_seedProvider);
                });
            });
        }

        [Fact]
        public async Task NotificationsIsProtected()
        {
            var requestUri = "/games/6a91eb4b-423a-41aa-8b5f-f5587260a4ed/stream";

            using (var response = await AuthorizedClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
            {
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task Notifications()
        {
            // Given:
            _seedProvider.Value = new Seed(42);

            var gamePath = await CreateGameAsync();
            var subscribePath = gamePath.ToString() + "/stream?player=dummy";

            using (var response = await AuthorizedClient.GetAsync(subscribePath, HttpCompletionOption.ResponseHeadersRead))
            {
                var body = new { card = new { deckNo = 2, suit = 1, rank = 9 }, column = 8, row = 0 };

                // When:
                using (var r = await AuthorizedClient.PostAsJsonAsync(gamePath, body))
                {
                    // Make sure the previous call succeeded to avoid getting stuck on reading stream.
                    Assert.True(r.IsSuccessStatusCode);
                }

                // Then:
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    var line0 = await reader.ReadLineAsync();
                    var line1 = await reader.ReadLineAsync();
                    var line2 = await reader.ReadLineAsync();

                    Assert.Equal("event: game-updated", line0);
                    Assert.StartsWith("data: {", line1);
                    Assert.Equal("", line2);
                }
            }
        }
    }
}
