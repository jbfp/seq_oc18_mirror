using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.Bots
{
    public sealed class BotTaskObserver : BackgroundService
    {
        private readonly IBotTaskObservable _observable;
        private readonly BotTaskHandler _handler;
        private readonly ILogger _logger;

        public BotTaskObserver(
            IBotTaskObservable observable,
            BotTaskHandler handler,
            ILogger<BotTaskObserver> logger)
        {
            _observable = observable ?? throw new ArgumentNullException(nameof(observable));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Subscribing to bot task observable");

            await _observable
                .SelectMany(botTask => Observable
                    .FromAsync(ct => _handler.HandleBotTaskAsync(botTask, ct)))
                .DefaultIfEmpty(Unit.Default);

            _logger.LogInformation("Bot task observable complete");
        }
    }
}
