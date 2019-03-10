using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.GetGameEvents
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameViewFeature(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddSingleton<IGameEventGeneratorProvider, PostgresGameEventGeneratorProvider>();
        }
    }
}
