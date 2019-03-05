using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.CreateGame
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCreateGameFeature(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddScoped<CreateGameHandler>()
                .AddSingleton<IRandomFactory, SystemRandomFactory>()
                .AddScoped<IGameStore, PostgresGameStore>();
        }
    }
}
