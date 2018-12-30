using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGame;
using Sequence.Core.GetGames;
using Sequence.Core.Notifications;
using Sequence.Core.Play;
using Sequence.Postgres;
using System;

namespace Sequence.Api
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSequence(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddPostgres(configuration);

            services.AddTransient<CreateGameHandler>();
            services.AddTransient<GetGameHandler>();
            services.AddTransient<GetGamesHandler>();
            services.AddTransient<PlayHandler>();

            var subscriptionHandler = new SubscriptionHandler();
            services.AddSingleton<SubscriptionHandler>(subscriptionHandler);
            services.AddSingleton<IGameUpdatedNotifier>(subscriptionHandler);

            services.AddTransient<ISeedProvider, RandomSeedProvider>();

            return services;
        }

        private static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PostgresOptions>(configuration.GetSection("Postgres"));
            services.AddSingleton<NpgsqlConnectionFactory>();

            services.AddTransient<IGameEventStore, PostgresGameEventStore>();
            services.AddTransient<IGameProvider, PostgresGameProvider>();
            services.AddTransient<IGameListProvider, PostgresGameListProvider>();
            services.AddTransient<IGameStore, PostgresGameStore>();

            services.AddSingleton<PostgresMigrations>();

            return services;
        }
    }
}
