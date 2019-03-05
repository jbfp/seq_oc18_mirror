using Dapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresGameEventListener : BackgroundService, IObservable<(GameId, GameEvent)>
    {
        private readonly NpgsqlConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private readonly Subject<(GameId, GameEvent)> _subject = new Subject<(GameId, GameEvent)>();

        public PostgresGameEventListener(
            NpgsqlConnectionFactory connectionFactory,
            ILogger<PostgresGameEventListener> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable Subscribe(IObserver<(GameId, GameEvent)> observer)
        {
            return _subject.Subscribe(observer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var connection = await _connectionFactory.CreateAndOpenAsync(stoppingToken))
                {
                    var notifications = Observable
                        .FromEvent<NotificationEventHandler, NpgsqlNotificationEventArgs>(
                            action => new NotificationEventHandler((_, args) => action(args)),
                            handler => connection.Notification += handler,
                            handler => connection.Notification -= handler)
                        .Select(args => args.AdditionalInformation)
                        .Select(JsonConvert.DeserializeObject<GameEventRow>)
                        .Select(MapToReturnType);

                    notifications.Subscribe(_subject);

                    var command = new CommandDefinition(
                        commandText: "LISTEN game_event_inserted;",
                        cancellationToken: stoppingToken);

                    _logger.LogInformation("Starting game_event_inserted listener");

                    await connection.ExecuteAsync(command);

                    _logger.LogInformation("Started game_event_inserted listener");

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
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception. Restarting loop");
                await Task.Delay(TimeSpan.FromSeconds(10)); // Give system a chance to recover.
                await ExecuteAsync(stoppingToken);
            }
        }

        private static (GameId, GameEvent) MapToReturnType(GameEventRow row)
        {
            var gameId = row.surrogate_game_id;

            var gameEvent = new GameEvent
            {
                ByPlayerId = row.by_player_id,
                CardDrawn = row.card_drawn?.ToCard(),
                CardUsed = row.card_used.ToCard(),
                Chip = row.chip,
                Coord = row.coord.ToCoord(),
                Index = row.idx,
                NextPlayerId = row.next_player_id,
                Sequences = row.sequences.Select(SequenceComposite.ToSequence).ToArray(),
                Winner = row.winner,
            };

            return (gameId, gameEvent);
        }
    }
}
