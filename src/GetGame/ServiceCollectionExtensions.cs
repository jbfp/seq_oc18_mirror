using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.GetGame
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameViewFeature(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<IGameProvider, PostgresGameProvider>();
        }
    }
}
