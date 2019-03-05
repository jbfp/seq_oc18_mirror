using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Bots;
using Sequence.Postgres;
using Sequence.Test.Postgres;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Test
{
    [Collection(PostgresDockerContainerCollection.Name)]
    public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        protected const string DefaultUserName = "test_player";

        private readonly BotTaskObservableStub _botTaskObservable = new BotTaskObservableStub();

        protected IntegrationTestBase(
            PostgresDockerContainerFixture fixture,
            WebApplicationFactory<Startup> factory)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));

            Factory = Factory.WithWebHostBuilder(builder =>
            {
                var db = fixture.CreateDatabaseAsync(CancellationToken.None).Result;

                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<NpgsqlConnectionFactory>(db);
                }).UseSetting("Postgres:ConnectionString", db.ConnectionString);
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

        private sealed class BotTaskObservableStub : IObservable<BotTask>
        {
            public IDisposable Subscribe(IObserver<BotTask> observer)
            {
                return Observable.Empty<BotTask>().Subscribe(observer);
            }
        }
    }
}
