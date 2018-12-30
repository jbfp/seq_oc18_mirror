using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.Bots
{
    public sealed class BotTaskObserver : BackgroundService, IObserver<BotTask>
    {
        private readonly BlockingCollection<BotTask> _tasks = new BlockingCollection<BotTask>();
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

        public override void Dispose()
        {
            base.Dispose();
            _tasks.Dispose();
        }

        public void OnCompleted()
        {
            _logger.LogInformation("No more bot tasks");
            _tasks.CompleteAdding();
        }

        public void OnError(Exception error)
        {
            _logger.LogCritical(error, "Got error from bot task observable");
            _tasks.CompleteAdding();
        }

        public void OnNext(BotTask task)
        {
            _logger.LogInformation(
                "Received bot task for {PlayerId} in {GameId}",
                task.GameId, task.Player);

            _tasks.Add(task);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Subscribing to bot task observable");

            using (_observable.Subscribe(this))
            {
                foreach (var task in _tasks.GetConsumingEnumerable(stoppingToken))
                {
                    try
                    {
                        await _handler.HandleBotTaskAsync(task, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Bot task handler threw an unhandled exception");
                    }
                }
            }
        }
    }
}
