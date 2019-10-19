using Microsoft.Extensions.DependencyInjection;

namespace Sequence.GetGame
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameViewFeature(this IServiceCollection services)
        {
            return services.AddSingleton<IGameProvider, PostgresGameProvider>();
        }
    }
}
