using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Postgres;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var builder = CreateWebHostBuilder(args);

                using (var host = builder.Build())
                {
                    using (var scope = host.Services.CreateScope())
                    {
                        var postgres = scope.ServiceProvider.GetService<PostgresMigrations>();

                        if (postgres != null)
                        {
                            Log.Information("Upgrading Postgres database if necessary");
                            await postgres.UpgradeDatabaseAsync(CancellationToken.None);
                        }
                    }

                    Log.Information("Starting web host");
                    await host.RunAsync(CancellationToken.None);
                    Log.Information("Goodbye");
                }

                return 0;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .SuppressStatusMessages(true)
            .UseSerilog((ctx, conf) => conf.ReadFrom.Configuration(ctx.Configuration))
            .UseUrls("http://localhost:5000");
    }
}
