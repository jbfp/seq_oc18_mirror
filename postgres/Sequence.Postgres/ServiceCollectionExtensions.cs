using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sequence.Core;
using Sequence.Core.Bots;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;
using Sequence.Core.Notifications;

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
            services.AddSingleton<IBotTaskObservable>(sp => sp.GetRequiredService<PostgresListener>());
            services.AddTransient<PostgresGameEventStore>();
            services.AddTransient<IGameProvider, PostgresGameProvider>();
            services.AddTransient<IGameListProvider, PostgresGameListProvider>();
            services.AddTransient<IGameStore, PostgresGameStore>();

            services.AddSingleton<PostgresMigrations>();

            services.AddTransient<IGameEventStore>(sp => new NotifyingGameEventStore(
                sp.GetRequiredService<PostgresGameEventStore>(),
                sp.GetRequiredService<IGameUpdatedNotifier>()));

            services.AddHealthChecks()
                .AddNpgSql(configSection["ConnectionString"]);

            return services;
        }
    }
}
