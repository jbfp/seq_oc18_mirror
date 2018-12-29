using Dapper;
using Microsoft.Extensions.Options;
using Sequence.Core;
using Sequence.Core.Play;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresGameEventStore : PostgresBase, IGameEventStore
    {
        public PostgresGameEventStore(IOptions<PostgresOptions> options) : base(options)
        {
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
            using (var transaction = connection.BeginTransaction())
            {
                int surrogateGameId;

                {
                    var command = new CommandDefinition(
                        commandText: "SELECT id FROM game WHERE game_id = @gameId;",
                        parameters: new { gameId },
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    surrogateGameId = await connection.QuerySingleAsync<int>(command);
                }

                // Couldn't figure out how to support INSERT with composite types with Dapper, so ADO.NET to the rescue.
                using (var command = connection.CreateCommand())
                {
                    var commandText = @"
                        INSERT INTO
                            game_event (game_id, idx, by_player_id, card_drawn, card_used, chip, coord, next_player_id, sequence)
                        VALUES
                            (@gameId, @idx, @byPlayerId, @cardDrawn, @cardUsed, @chip, @coord, @nextPlayerId, @sequence);";

                    command.CommandText = commandText;
                    command.Parameters.AddWithValue("@gameId", surrogateGameId);
                    command.Parameters.AddWithValue("@idx", gameEvent.Index);
                    command.Parameters.AddWithValue("@byPlayerId", gameEvent.ByPlayerId.ToInt32());
                    command.Parameters.AddWithValue("@cardDrawn", (object)CardComposite.FromCard(gameEvent.CardDrawn) ?? DBNull.Value);
                    command.Parameters.AddWithValue("@cardUsed", CardComposite.FromCard(gameEvent.CardUsed));
                    command.Parameters.AddWithValue("@chip", (object)gameEvent.Chip ?? DBNull.Value);
                    command.Parameters.AddWithValue("@coord", CoordComposite.FromCoord(gameEvent.Coord));
                    command.Parameters.AddWithValue("@nextPlayerId", (object)gameEvent.NextPlayerId?.ToInt32() ?? DBNull.Value);
                    command.Parameters.AddWithValue("@sequence", (object)SequenceComposite.FromSequence(gameEvent.Sequence) ?? DBNull.Value);
                    command.Transaction = transaction;

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
        }
    }
}
