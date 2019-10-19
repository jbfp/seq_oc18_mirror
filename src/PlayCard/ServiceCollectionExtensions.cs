using Microsoft.Extensions.DependencyInjection;

namespace Sequence.PlayCard
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlayCardFeature(this IServiceCollection services)
        {
            return services
                .AddScoped<PlayCardHandler>()
                .AddScoped<IGameEventStore, PostgresGameEventStore>();
        }
    }
}
