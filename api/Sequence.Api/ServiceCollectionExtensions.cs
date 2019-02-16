using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sequence.Api.Caching;
using Sequence.Core;
using Sequence.Core.Bots;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGame;
using Sequence.Core.GetGames;
using Sequence.Core.Play;
using System;

namespace Sequence.Api
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSequence(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            services.AddTransient<BotTaskHandler>();
            services.AddTransient<CreateGameHandler>();
            services.AddTransient<GetGameHandler>();
            services.AddTransient<GetGamesHandler>();
            services.AddTransient<PlayHandler>();

            services.AddTransient<ISeedProvider, RandomSeedProvider>();

            services.AddHostedService<BotTaskObserver>();

            services.Decorate<IGameProvider, CachedGameStore>();
            services.Decorate<IGameEventStore, CachedGameEventStore>();
            services.Decorate<IGameEventStore, SignalRGameEventStoreDecorator>();

            return services;
        }
    }
}
