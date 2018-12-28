using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;
using Sequence.Core.Play;
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
    public sealed class IntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly InMemoryDatabase _database = new InMemoryDatabase();
        private readonly SeedProviderStub _seedProvider = new SeedProviderStub();
        private readonly WebApplicationFactory<Startup> _factory;

        public IntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IGameEventStore>(_database);
                    services.AddSingleton<IGameProvider>(_database);
                    services.AddSingleton<IGameListProvider>(_database);
                    services.AddSingleton<IGameStore>(_database);
                    services.AddSingleton<ISeedProvider>(_seedProvider);
                    services.AddLogging(options =>
                    {
                        options.AddFilter("Default", LogLevel.Warning);
                        options.AddFilter("Microsoft", LogLevel.Warning);
                        options.AddFilter("System", LogLevel.Warning);
                    });
                });
            });
        }

        [Theory]
        [InlineData("/games")]
        [InlineData("/games/6a91eb4b-423a-41aa-8b5f-f5587260a4ed")]
        [InlineData("/bots")]
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
                opponents = new[]
                {
                    new { name = "test", type = 1 },
                },
            };

            var client = _factory.CreateClient();

            // When:
            var response = await client.PostAsJsonAsync("/games", body);

            // Then:
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateGameReturnsCreated()
        {
            // Given:
            var body = new
            {
                opponents = new[]
                {
                    new { name = "test", type = 1 },
                },
            };

            var client = CreateAuthorizedClient();

            // When:
            var response = await client.PostAsJsonAsync("/games", body);

            // Then:
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.NotNull(response.Content);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task CreateGameReturnsBadRequestIfGameSizeIsInvalid()
        {
            // Given:
            var playerId = "test_player";

            var body = new
            {
                opponents = new[]
                {
                    new { name = "fail1", type = 1 },
                    new { name = "fail2", type = 1 },
                    new { name = "fail3", type = 1 },
                    new { name = "fail4", type = 1 },
                },
            };

            var client = CreateAuthorizedClient(playerId);

            // When:
            var response = await client.PostAsJsonAsync("/games", body);

            // Then:
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeAnonymousType(json, new { error = "" });
            Assert.Equal("Game size is invalid.", obj.error);
        }

        [Fact]
        public async Task CreateGameReturnsBadRequestIfPlayer1AndPlayer2AreSame()
        {
            // Given:
            var playerId = "test_player";

            var body = new
            {
                opponents = new[]
                {
                    new { name = playerId, type = 1 },
                },
            };

            var client = CreateAuthorizedClient(playerId);

            // When:
            var response = await client.PostAsJsonAsync("/games", body);

            // Then:
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeAnonymousType(json, new { error = "" });
            Assert.Equal("Duplicate players are not allowed.", obj.error);
        }

        [Fact]
        public async Task PlayIsProtected()
        {
            // Given:
            var client = _factory.CreateClient();
            var body = new { card = new { deckNo = 0, suit = 0, rank = 0 }, column = 0, row = 0 };

            // When:
            var response = await client.PostAsJsonAsync("/games/6a91eb4b-423a-41aa-8b5f-f5587260a4ed", body);

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
            var response = await client.PostAsJsonAsync("/games/idontexistforsure", body);

            // Then:
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetGameReturnsNotFound()
        {
            // Given:
            var client = CreateAuthorizedClient();

            // When:
            var response = await client.GetAsync("/games/idontexistforsure");

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
            var response = await client.GetAsync("/games");

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
            var result = JObject.Parse(await client.GetStringAsync("/games"));

            // Then:
            var games = result["games"].ToObject<JArray>();
            Assert.NotEmpty(games);
            var game = games[0].ToObject<JObject>();
            Assert.NotNull(game["gameId"]);
            Assert.Equal("test_player", game["currentPlayer"].ToObject<string>());
            Assert.Equal(new[] { "test" }, game["opponents"].ToObject<string[]>());
        }

        [Fact]
        public async Task NotificationsIsProtected()
        {
            // Given:
            var client = _factory.CreateClient();

            // When:
            var response = await client.GetAsync(
                "/games/6a91eb4b-423a-41aa-8b5f-f5587260a4ed/stream",
                HttpCompletionOption.ResponseHeadersRead);

            // Then:
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Notifications()
        {
            // Given:
            _seedProvider.Value = new Seed(42);
            var gamePath = await CreateGameAsync();
            var subscribePath = gamePath.ToString() + "/stream?player=dummy";
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync(subscribePath, HttpCompletionOption.ResponseHeadersRead);
            var body = new { card = new { deckNo = 1, suit = 1, rank = 9 }, column = 8, row = 0 };

            // When:
            var r = await client.PostAsJsonAsync(gamePath, body);
            Assert.True(r.IsSuccessStatusCode); // Make sure the previous call succeeded to avoid getting stuck on reading stream.

            // Then:
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                var line0 = await reader.ReadLineAsync();
                var line1 = await reader.ReadLineAsync();
                var line2 = await reader.ReadLineAsync();

                Assert.Equal("event: game-updated", line0);
                Assert.Equal("data: 1", line1);
                Assert.Equal("", line2);
            }
        }

        [Fact]
        public async Task GetBotsReturnsCorrectResult()
        {
            // Given:
            var client = CreateAuthorizedClient();

            // When:
            var response = await client.GetAsync("/bots");

            // Then:
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotEmpty(json["botTypes"]);
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

            var form = new
            {
                opponents = new[]
                {
                    new { name = opponent, type = 1 },
                },
            };

            var response = await client.PostAsJsonAsync("/games", form);

            return response.Headers.Location;
        }

        private sealed class SeedProviderStub : ISeedProvider
        {
            public Seed Value { get; set; } = new Seed(42);

            public Task<Seed> GenerateSeedAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(Value);
            }
        }
    }
}
