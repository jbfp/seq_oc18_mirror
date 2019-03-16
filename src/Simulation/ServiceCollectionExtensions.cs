using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.Simulation
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimulationFeature(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddScoped<SimulationHandler>()
                .AddScoped<ISimulationStore, PostgresSimulationStore>();
        }
    }
}
