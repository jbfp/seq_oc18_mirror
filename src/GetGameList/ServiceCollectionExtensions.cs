using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.GetGameList
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameListFeature(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddScoped<GetGameListHandler>()
                .AddScoped<IGameListProvider, PostgresGameListProvider>();
        }
    }
}
