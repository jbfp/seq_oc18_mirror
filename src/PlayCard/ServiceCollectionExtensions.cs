using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.PlayCard
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlayCardFeature(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddScoped<PlayCardHandler>()
                .AddScoped<IGameEventStore, PostgresGameEventStore>();
        }
    }
}
