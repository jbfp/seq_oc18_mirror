using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Sqlite;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(new CompactJsonFormatter(), "./log.json")
                .CreateLogger();

            try
            {
                Log.Information("Building web host");
                var builder = CreateWebHostBuilder(args);

                using (var host = builder.Build())
                {
                    using (var scope = host.Services.CreateScope())
                    {
                        var sqlite = scope.ServiceProvider.GetService<SqliteConnectionFactory>();

                        if (sqlite != null)
                        {
                            Log.Information("Upgrading SQLite database if necessary");
                            await SqliteMigrations.UpgradeDatabaseAsync(sqlite, CancellationToken.None);
                        }
                    }

                    Log.Information("Starting web host");
                    await host.RunAsync(CancellationToken.None);
                }

                Log.Information("Goodbye");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseSerilog();
    }
}
