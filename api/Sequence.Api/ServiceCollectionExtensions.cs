using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sequence.Core.Bots;
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

            services.AddTransient<BotTaskHandler>();
            services.AddTransient<CreateGameHandler>();
            services.AddTransient<GetGameHandler>();
            services.AddTransient<GetGamesHandler>();
            services.AddTransient<PlayHandler>();

            var subscriptionHandler = new SubscriptionHandler();
            services.AddSingleton<SubscriptionHandler>(subscriptionHandler);
            services.AddSingleton<IGameUpdatedNotifier>(subscriptionHandler);

            services.AddTransient<ISeedProvider, RandomSeedProvider>();

            services.AddHostedService<BotTaskObserver>();

            return services;
        }
    }
}
