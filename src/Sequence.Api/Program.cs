using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Api.Sqlite;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = CreateWebHostBuilder(args);

            using (var host = builder.Build())
            {
                using (var scope = host.Services.CreateScope())
                {
                    var sqlite = scope.ServiceProvider.GetService<SqliteConnectionFactory>();

                    if (sqlite != null)
                    {
                        await SqliteMigrations.UpgradeDatabaseAsync(sqlite, CancellationToken.None);
                    }
                }

                await host.RunAsync(CancellationToken.None);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>();
    }
}
