using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGame;
using Sequence.Core.GetGames;
using Sequence.Core.Play;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Sqlite
{
    [Obsolete]
    public sealed class SqliteDb : IGameEventStore, IGameProvider, IGameListProvider, IGameStore
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public SqliteDb(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task AddEventAsync(GameId gameId, GameEvent gameEvent, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = await CreateAndOpenAsync(cancellationToken))
            {
                var commandText = @"
                    INSERT INTO
                        game_events (game_id, idx, by_player_id, card_drawn, card_used, chip, coord, next_player_id)
                    VALUES
                        (@gameId, @idx, @byPlayerId, @cardDrawn, @cardUsed, @chip, @coord, @nextPlayerId);";

                var parameters = new
                {
                    gameId = gameId.ToString(),
                    idx = gameEvent.Index,
                    byPlayerId = gameEvent.ByPlayerId.ToString(),
                    cardDrawn = gameEvent.CardDrawn == null ? null : JsonConvert.SerializeObject(gameEvent.CardDrawn),
                    cardUsed = JsonConvert.SerializeObject(gameEvent.CardUsed),
                    chip = gameEvent.Chip,
                    coord = JsonConvert.SerializeObject(gameEvent.Coord),
                    nextPlayerId = gameEvent.NextPlayerId.ToString(),
                };

                var command = new CommandDefinition(
                    commandText,
                    parameters,
                    cancellationToken: cancellationToken
                );

                await connection.ExecuteAsync(command);
            }
        }

        public async Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            GameInit init;
            GameEvent[] gameEvents;

            using (var connection = await CreateAndOpenAsync(cancellationToken))
            {
                {
                    var command = new CommandDefinition(
                        commandText: "SELECT player1, player2, seed FROM games WHERE game_id = @gameId;",
                        parameters: new { gameId = gameId.ToString() },
                        cancellationToken: cancellationToken
                    );

                    var result = await connection.QuerySingleOrDefaultAsync<dynamic>(command);

                    if (result == null)
                    {
                        return null;
                    }

                    init = new GameInit(
                        new PlayerId((string)result.player1),
                        new PlayerId((string)result.player2),
                        new Seed((int)result.seed)
                    );
                }

                {
                    var command = new CommandDefinition(
                        commandText: "SELECT * FROM game_events WHERE game_id = @gameId;",
                        parameters: new { gameId = gameId.ToString() },
                        cancellationToken: cancellationToken
                    );

                    var rows = await connection.QueryAsync(command);

                    gameEvents = rows.Select(row => new GameEvent
                    {
                        ByPlayerId = new PlayerId((string)row.by_player_id),
                        CardDrawn = row.card_drawn == null ? null : JsonConvert.DeserializeObject<Card>((string)row.card_drawn),
                        CardUsed = JsonConvert.DeserializeObject<Card>((string)row.card_used),
                        Chip = (Team?)row.chip,
                        Coord = JsonConvert.DeserializeObject<Coord>((string)row.coord),
                        Index = (int)row.idx,
                        NextPlayerId = new PlayerId((string)row.next_player_id),
                    }).ToArray();
                }
            }

            return new Game(init, gameEvents);
        }

        public Task<GameList> GetGamesForPlayerAsync(PlayerId playerId, CancellationToken cancellationToken)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            throw new NotImplementedException();
        }

        public async Task<GameId> PersistNewGameAsync(NewGame newGame, CancellationToken cancellationToken)
        {
            if (newGame == null)
            {
                throw new ArgumentNullException(nameof(newGame));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var gameId = Guid.NewGuid().ToString();

            using (var connection = await CreateAndOpenAsync(cancellationToken))
            {
                var commandText = @"
                    INSERT INTO
                        games (game_id, player1, player2, seed, version)
                    VALUES
                        (@gameId, @player1, @player2, @seed, @version);";

                var parameters = new
                {
                    gameId,
                    player1 = newGame.Player1.ToString(),
                    player2 = newGame.Player2.ToString(),
                    seed = newGame.Seed.ToInt32(),
                    version = 1,
                };

                var command = new CommandDefinition(
                    commandText,
                    parameters,
                    cancellationToken: cancellationToken
                );

                await connection.ExecuteAsync(command);
            }

            return new GameId(gameId);
        }

        private async Task<SqliteConnection> CreateAndOpenAsync(CancellationToken cancellationToken)
        {
            var connection = _connectionFactory();

            Trace.Assert(connection != null);

            await connection.OpenAsync(cancellationToken);

            // Enable foreign keys.
            {
                var command = new CommandDefinition(
                    commandText: "PRAGMA foreign_keys = ON;",
                    cancellationToken: cancellationToken
                );

                await connection.ExecuteAsync(command);
            }

            return connection;
        }
    }
}
