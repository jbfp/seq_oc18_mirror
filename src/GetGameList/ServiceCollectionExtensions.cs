using Microsoft.Extensions.DependencyInjection;

namespace Sequence.GetGameList
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameListFeature(this IServiceCollection services)
        {
            return services
                .AddScoped<GetGameListHandler>()
                .AddScoped<IGameListProvider, PostgresGameListProvider>();
        }
    }
}
