using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Sequence.Core;
using Sequence.Core.Bots;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresListener : BackgroundService, IBotTaskObservable
    {
        private readonly NpgsqlConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        private readonly ReplaySubject<BotTask> _subject = new ReplaySubject<BotTask>(
            window: TimeSpan.FromSeconds(30));

        public PostgresListener(
            NpgsqlConnectionFactory connectionFactory,
            ILogger<PostgresListener> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable Subscribe(IObserver<BotTask> observer)
        {
            return _subject.Subscribe(observer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var connection = await _connectionFactory.CreateAndOpenAsync(stoppingToken))
            {
                var notifications = Observable
                    .FromEvent<NotificationEventHandler, NpgsqlNotificationEventArgs>(
                        action => new NotificationEventHandler((_, args) => action(args)),
                        handler => connection.Notification += handler,
                        handler => connection.Notification -= handler)
                    .Select(ParseNotificationBody)
                    .SelectMany(GetLatestBotTaskForGame)
                    .Replay(window: TimeSpan.FromSeconds(30));

                notifications.Connect();

                var command = new CommandDefinition(
                    commandText: "LISTEN game_event_inserted;",
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Starting game_event_inserted listener");

                await connection.ExecuteAsync(command);

                foreach (var botTask in await GetInitialBotTasksAsync(stoppingToken))
                {
                    _subject.OnNext(botTask);
                }

                notifications.Subscribe(_subject);

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
                    _subject.OnError(ex);
                    throw;
                }

                _subject.OnCompleted();
            }
        }

        private async Task<IImmutableList<BotTask>> GetInitialBotTasksAsync(CancellationToken cancellationToken)
        {
            using (var connection = await _connectionFactory.CreateAndOpenAsync(cancellationToken))
            {
                _logger.LogInformation("Getting initial bot tasks");

                const string sql = @"
                    SELECT
                      DISTINCT(g.game_id)
                    , gp.player_id
                    FROM public.game_player AS gp

                    INNER JOIN (
                        SELECT DISTINCT ON (game_id) * FROM
                        public.game_event
                        ORDER BY game_id ASC, idx DESC
                    ) AS ge
                    ON ge.game_id = gp.game_id

                    INNER JOIN public.game AS g
                    ON g.id = gp.game_id

                    WHERE gp.player_type = 'bot'
                    AND ge.next_player_id = gp.id;";

                var command = new CommandDefinition(
                    commandText: sql,
                    cancellationToken: cancellationToken);

                var rows = (await connection.QueryAsync<LatestBotTaskForGame>(command)).AsList();

                _logger.LogInformation("Got {NumTasks} initial bot tasks", rows.Count);

                return rows
                    .Select(row => new BotTask(row.game_id, row.player_id))
                    .ToImmutableList();
            }
        }

        private static int ParseNotificationBody(NpgsqlNotificationEventArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var json = args.AdditionalInformation;
            var row = JsonConvert.DeserializeAnonymousType(json, new { game_id = 0 });
            return row.game_id;
        }

        private IObservable<BotTask> GetLatestBotTaskForGame(int gameId)
        {
            return Observable.Using(_connectionFactory.CreateAndOpenAsync, async (connection, cancellationToken) =>
            {
                _logger.LogInformation("Getting latest bot task for game {GameId}", gameId);

                const string sql = @"
                    SELECT
                      g.game_id
                    , gp.player_id
                    FROM public.game_player AS gp

                    INNER JOIN (
                        SELECT DISTINCT ON (game_id) * FROM
                        public.game_event
                        ORDER BY game_id ASC, idx DESC
                    ) AS ge
                    ON ge.game_id = gp.game_id

                    INNER JOIN public.game AS g
                    ON g.id = gp.game_id

                    WHERE gp.player_type = 'bot'
                    AND ge.next_player_id = gp.id
                    AND g.id = @gameId;";

                var command = new CommandDefinition(
                    commandText: sql,
                    parameters: new { gameId },
                    cancellationToken: cancellationToken);

                var row = await connection.QuerySingleOrDefaultAsync<LatestBotTaskForGame>(command);

                if (row == null)
                {
                    _logger.LogWarning("Found no bot tasks for game {GameId}", gameId);
                    return Observable.Empty<BotTask>();
                }

                _logger.LogInformation(
                    "Found bot task for player {playerId} in game {gameId}",
                    row.player_id, row.game_id);

                return Observable.Return(new BotTask(row.game_id, row.player_id));
            });
        }


#pragma warning disable CS0649
        private sealed class LatestBotTaskForGame
        {
            public GameId game_id;
            public PlayerHandle player_id;
        }
#pragma warning restore CS0649
    }
}
