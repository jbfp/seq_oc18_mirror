using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sequence.Core;
using Sequence.Core.Bots;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;

namespace Sequence.Postgres
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("Postgres");

            services.Configure<PostgresOptions>(configSection);
            services.AddSingleton<NpgsqlConnectionFactory>();
            services.AddSingleton<PostgresListener>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<PostgresListener>());
            services.AddTransient<IBotGameProvider, PostgresGameProvider>();
            services.AddSingleton<IBotTaskObservable>(sp => sp.GetRequiredService<PostgresListener>());
            services.AddTransient<IGameEventStore, PostgresGameEventStore>();
            services.AddTransient<IGameProvider, PostgresGameProvider>();
            services.AddTransient<IGameListProvider, PostgresGameListProvider>();
            services.AddTransient<IGameStore, PostgresGameStore>();

            services.AddSingleton<PostgresMigrations>();

            services.AddHealthChecks()
                .AddNpgSql(configSection["ConnectionString"]);

            return services;
        }
    }
}
