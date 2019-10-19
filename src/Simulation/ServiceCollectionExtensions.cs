using Microsoft.Extensions.DependencyInjection;

namespace Sequence.Simulation
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimulationFeature(this IServiceCollection services)
        {
            return services
                .AddScoped<SimulationHandler>()
                .AddScoped<ISimulationStore, PostgresSimulationStore>();
        }
    }
}
