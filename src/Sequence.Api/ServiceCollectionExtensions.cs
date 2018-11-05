using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Api.Sqlite;
using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGame;
using Sequence.Core.GetGames;
using Sequence.Core.Notifications;
using Sequence.Core.Play;
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

            services.AddSqlite(configuration);

            services.AddTransient<CreateGameHandler>();
            services.AddTransient<GetGameHandler>();
            services.AddTransient<GetGamesHandler>();
            services.AddTransient<PlayHandler>();

            var subscriptionHandler = new SubscriptionHandler();
            services.AddSingleton<SubscriptionHandler>(subscriptionHandler);
            services.AddSingleton<IGameUpdatedNotifier>(subscriptionHandler);

            return services;
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbOptions>(configuration.GetSection("MongoDb"));

            services.AddTransient<IGameEventStore, MongoDb>();
            services.AddTransient<IGameProvider, MongoDb>();
            services.AddTransient<IGameListProvider, MongoDb>();
            services.AddTransient<IGameStore, MongoDb>();

            return services;
        }

        private static IServiceCollection AddSqlite(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration
                .GetSection("Sqlite")
                .GetValue<string>("ConnectionString");

            var connectionFactory = new SqliteConnectionFactory(() => new SqliteConnection(connectionString));
            var sqlite = new SqliteDb(connectionFactory);
            var cache = new CachedGameStore(sqlite, sqlite);

            services.AddSingleton<IGameEventStore>(cache);
            services.AddSingleton<IGameProvider>(cache);
            services.AddSingleton<IGameListProvider>(sqlite);
            services.AddSingleton<IGameStore>(sqlite);

            return services;
        }
    }
}
