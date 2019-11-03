using Dapper;
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
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IDisposable Subscribe(IObserver<(GameId, GameEvent)> observer)
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

            var gameEvent = new GameEvent(
                byPlayerId: row.by_player_id,
                cardDrawn: row.card_drawn?.ToCard(),
                cardUsed: row.card_used.ToCard(),
                chip: row.chip,
                coord: row.coord.ToCoord(),
                index: row.idx,
                nextPlayerId: row.next_player_id,
                sequences: row.sequences.Select(SequenceComposite.ToSequence).ToImmutableArray(),
                winner: row.winner);

            return (gameId, gameEvent);
        }
    }
}
