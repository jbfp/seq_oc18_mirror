using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api.Sqlite
{
    public static class SqliteMigrations
    {
        public static async Task UpgradeDatabaseAsync(
            SqliteConnectionFactory connectionFactory,
            CancellationToken cancellationToken)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = connectionFactory())
            {
                await connection.OpenAsync(cancellationToken);

                int version;

                // Get current db version.
                {
                    var command = new CommandDefinition(
                        commandText: "PRAGMA user_version;",
                        cancellationToken: cancellationToken
                    );

                    version = await connection.QuerySingleOrDefaultAsync<int>(command);
                }

                if (version == 0)
                {
                    var commandText = @"
                        CREATE TABLE games
                        ( game_id TEXT NOT NULL UNIQUE
                        , player1 TEXT NOT NULL
                        , player2 TEXT NOT NULL
                        , seed INTEGER NOT NULL
                        , version INTEGER NOT NULL
                        );

                        CREATE TABLE game_events
                        ( game_id TEXT NOT NULL REFERENCES games (game_id)
                        , idx INTEGER NOT NULL
                        , by_player_id TEXT NOT NULL
                        , card_drawn TEXT NULL
                        , card_used TEXT NOT NULL
                        , chip INTEGER NULL
                        , coord TEXT NOT NULL
                        , next_player_id TEXT NULL
                        , UNIQUE (game_id, idx)
                        );
                    ";

                    var command = new CommandDefinition(
                        commandText: commandText,
                        cancellationToken: cancellationToken
                    );

                    await connection.ExecuteAsync(command);

                    version = 1;
                }

                // Update version in db header.
                {
                    var command = new CommandDefinition(
                        commandText: $"PRAGMA user_version = {version};", // Cannot use parameters in PRAGMA statement.
                        cancellationToken: cancellationToken
                    );

                    await connection.ExecuteAsync(command);
                }
            }
        }
    }
}
