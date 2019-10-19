using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Sequence.Bots
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBotsFeature(this IServiceCollection services)
        {
            return services
                .AddTransient<BotTaskHandler>()
                .AddHostedService<BotTaskObserver>()
                .AddSingleton<PostgresListener>()
                .AddSingleton<IHostedService>(sp => sp.GetRequiredService<PostgresListener>())
                .AddSingleton<IObservable<BotTask>>(sp =>
                    sp.GetRequiredService<PostgresListener>());
        }
    }
}
