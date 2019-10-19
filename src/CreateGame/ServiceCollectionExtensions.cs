using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.CreateGame
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCreateGameFeature(this IServiceCollection services)
        {
            return services
                .AddScoped<CreateGameHandler>()
                .AddSingleton<IRandomFactory, SystemRandomFactory>()
                .AddScoped<IGameStore, PostgresGameStore>();
        }
    }
}
