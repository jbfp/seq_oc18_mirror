using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sequence.Api.Sqlite;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Api.Test
{
    public sealed class IntegrationTests : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly InMemorySqlite _sqlite = new InMemorySqlite();
        private readonly WebApplicationFactory<Startup> _factory;

        public IntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<SqliteConnectionFactory>(_sqlite.CreateConnection);
                });
            });
        }

        public void Dispose()
        {
            _sqlite.Dispose();
        }

        [Theory]
        [InlineData("/api/games")]
        [InlineData("/api/games/123456")]
        public async Task GetResourceIsProtected(string path)
        {
            // Given:
            var client = _factory.CreateClient();

            // When:
            var response = await client.GetAsync(path);

            // Then:
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateGameIsProtected()
        {
            // Given:
            var body = new
            {
                opponent = "test"
            };

            var client = _factory.CreateClient();

            // When:
            var response = await client.PostAsJsonAsync("/api/games", body);

            // Then:
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateGameReturnsCreated()
        {
            // Given:
            var body = new
            {
                opponent = "test"
            };

            var client = CreateAuthorizedClient();

            // When:
            var response = await client.PostAsJsonAsync("/api/games", body);

            // Then:
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.NotNull(response.Content);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task PlayIsProtected()
        {
            // Given:
            var client = _factory.CreateClient();
            var body = new { card = new { deckNo = 0, suit = 0, rank = 0 }, column = 0, row = 0 };

            // When:
            var response = await client.PostAsJsonAsync("/api/games/1234", body);

            // Then:
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PlayReturnsNotFound()
        {
            // Given:
            var client = CreateAuthorizedClient();
            var body = new { card = new { deckNo = 0, suit = 0, rank = 0 }, column = 0, row = 0 };

            // When:
            var response = await client.PostAsJsonAsync("/api/games/idontexistforsure", body);

            // Then:
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetGameReturnsNotFound()
        {
            // Given:
            var client = CreateAuthorizedClient();

            // When:
            var response = await client.GetAsync("/api/games/idontexistforsure");

            // Then:
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetGameReturnsOk()
        {
            // Given:
            var gamePath = await CreateGameAsync();
            var client = CreateAuthorizedClient();

            // When:
            var response = await client.GetAsync(gamePath);

            // Then:
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetGamesReturnsOk()
        {
            // Given:
            var client = CreateAuthorizedClient();

            // When:
            var response = await client.GetAsync("/api/games");

            // Then:
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetGamesReturnsGames()
        {
            // Given:
            await CreateGameAsync();
            var client = CreateAuthorizedClient();

            // When:
            var result = JObject.Parse(await client.GetStringAsync("/api/games"));

            // Then:
            Assert.NotEmpty(result["gameIds"].ToObject<string[]>());
        }

        [Fact]
        public async Task NotificationsIsProtected()
        {
            // Given:
            var client = _factory.CreateClient();

            // When:
            var response = await client.GetAsync("/api/games/1234/stream", HttpCompletionOption.ResponseHeadersRead);

            // Then:
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Notifications()
        {
            // Given:
            var gamePath = await CreateGameAsync();
            var subscribePath = gamePath.ToString() + "/stream?playerId=dummy";
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync(subscribePath, HttpCompletionOption.ResponseHeadersRead);
            var body = new { card = new { deckNo = 0, suit = 0, rank = 0 }, column = 0, row = 0 };

            // When:
            var r = await client.PostAsJsonAsync(gamePath, body);
            Assert.True(r.IsSuccessStatusCode); // Make sure the previous call succeeded to avoid getting stuck on reading stream.

            // Then:
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                var expected = "event:game-updated";
                var actual = await reader.ReadLineAsync();
                Assert.Equal(expected, actual);
            }
        }

        private HttpClient CreateAuthorizedClient(string playerId = "test_player")
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("test_player");
            return client;
        }

        private async Task<Uri> CreateGameAsync(string opponent = "test")
        {
            var client = CreateAuthorizedClient();
            var form = new { opponent };
            var response = await client.PostAsJsonAsync("/api/games", form);
            return response.Headers.Location;
        }
    }
}
