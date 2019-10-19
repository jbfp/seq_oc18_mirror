using Microsoft.Extensions.DependencyInjection;
using Sequence.PlayCard;

namespace Sequence.RealTime
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRealTimeFeature(this IServiceCollection services)
        {
            return services.AddTransient<IRealTimeContext, GameHubRealTimeContext>();
        }
    }
}
