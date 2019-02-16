using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Core;
using Sequence.Core.Bots;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Api.Test.IntegrationTests
{
    public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        protected const string DefaultUserName = "test_player";

        private readonly InMemoryDatabase _database = new InMemoryDatabase();
        private readonly SeedProviderStub _seedProvider = new SeedProviderStub();
        private readonly BotTaskObservableStub _botTaskObservable = new BotTaskObservableStub();

        protected IntegrationTestBase(WebApplicationFactory<Startup> factory)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));

            Factory = Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<InMemoryDatabase>(_database);
                    services.AddSingleton<IBotTaskObservable>(_botTaskObservable);
                    services.AddSingleton<IGameProvider>(_database);
                    services.AddSingleton<IGameListProvider>(_database);
                    services.AddSingleton<IGameStore>(_database);
                    services.AddSingleton<ISeedProvider>(_seedProvider);
                });
            });
        }

        protected WebApplicationFactory<Startup> Factory { get; set; }

        protected HttpClient AuthorizedClient
        {
            get
            {
                var client = Factory.CreateDefaultClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(DefaultUserName);
                return client;
            }
        }
        protected HttpClient UnauthorizedClient => Factory.CreateDefaultClient();

        protected async Task<Uri> CreateGameAsync(string opponent = "test")
        {
            var form = new
            {
                boardType = 0,
                numSequencesToWin = 1,
                opponents = new[]
                {
                    new { name = opponent, type = 0 },
                },
            };

            var response = await AuthorizedClient.PostAsJsonAsync("/games", form);

            return response.Headers.Location;
        }

        private sealed class BotTaskObservableStub : IBotTaskObservable
        {
            public IDisposable Subscribe(IObserver<BotTask> observer)
            {
                return Observable.Empty<BotTask>().Subscribe(observer);
            }
        }
    }
}
