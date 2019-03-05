using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.RealTime
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRealTimeFeature(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddHostedService<SignalRNotifier>();
        }
    }
}
