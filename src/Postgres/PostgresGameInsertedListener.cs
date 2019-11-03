using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresGameInsertedListener : BackgroundService, IObservable<GameId>
    {
        private readonly NpgsqlConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private readonly Subject<GameId> _subject = new Subject<GameId>();

        public PostgresGameInsertedListener(
            NpgsqlConnectionFactory connectionFactory,
            ILogger<PostgresGameInsertedListener> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IDisposable Subscribe(IObserver<GameId> observer)
        {
            return _subject.Subscribe(observer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var connection = await _connectionFactory.CreateAndOpenAsync(stoppingToken);
                var notifications = Observable
                    .FromEvent<NotificationEventHandler, NpgsqlNotificationEventArgs>(
                        action => new NotificationEventHandler((_, args) => action(args)),
                        handler => connection.Notification += handler,
                        handler => connection.Notification -= handler)
                    .Select(args => args.Payload)
                    .Select(JsonConvert.DeserializeObject<GameRow>)
                    .Select(row => row.game_id);

                notifications.Subscribe(_subject);

                var command = new CommandDefinition(
                    commandText: "LISTEN game_inserted;",
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Starting game_inserted listener");

                await connection.ExecuteAsync(command);

                _logger.LogInformation("Started game_inserted listener");

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await connection.WaitAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred whilst listening for game events");
                }

                _subject.OnCompleted();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception. Restarting loop");
                await Task.Delay(TimeSpan.FromSeconds(10)); // Give system a chance to recover.
                await ExecuteAsync(stoppingToken);
            }
        }

#pragma warning disable CS0649, CS8618
        private sealed class GameRow
        {
            public GameId game_id;
        }
#pragma warning restore CS0649, CS8618
    }
}
