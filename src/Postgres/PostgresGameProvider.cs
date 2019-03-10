using Dapper;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresGameProvider
    {
        private readonly NpgsqlConnectionFactory _connectionFactory;

        public PostgresGameProvider(NpgsqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<Tuple<GameInit, GameEvent[]>> GetById(
            GameId gameId,
            CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            GameInit init;
            GameEvent[] gameEvents;

            using (var connection = await _connectionFactory.CreateAndOpenAsync(cancellationToken))
            using (var transaction = connection.BeginTransaction())
            {
                {
                    get_game_init_by_id[] rows;

                    var commandText = @"
                        SELECT
                          g.first_player_id AS first_player_id
                        , gp.id AS player_id
                        , gp.player_id AS player_handle
                        , gp.player_type AS player_type
                        , g.board_type
                        , g.num_sequences_to_win
                        , g.seed
                        FROM public.game AS g

                        INNER JOIN public.game_player AS gp
                        ON gp.game_id = g.id

                        WHERE g.game_id = @gameId

                        ORDER BY gp.id ASC;";

                    var command = new CommandDefinition(
                        commandText,
                        parameters: new { gameId },
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    rows = await connection
                        .QueryAsync<get_game_init_by_id>(command)
                        .ContinueWith(t => t.Result.ToArray());

                    if (rows.Length == 0)
                    {
                        return null;
                    }

                    init = new GameInit(
                        players: rows.Select(row => new Player(row.player_id, row.player_handle, row.player_type)).ToImmutableList(),
                        firstPlayerId: rows[0].first_player_id,
                        seed: rows[0].seed,
                        boardType: rows[0].board_type,
                        numSequencesToWin: rows[0].num_sequences_to_win
                    );
                }

                {
                    var commandText = @"
                        SELECT
                          ge.idx
                        , ge.by_player_id
                        , ge.card_drawn
                        , ge.card_used
                        , ge.chip
                        , ge.coord
                        , ge.next_player_id
                        , ge.sequences
                        , ge.winner
                        FROM public.game_event AS ge
                        INNER JOIN public.game AS g ON g.id = ge.game_id
                        WHERE g.game_id = @gameId
                        ORDER BY idx ASC;";

                    var parameters = new { gameId = gameId };

                    var command = new CommandDefinition(
                        commandText,
                        parameters,
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    var rows = await connection.QueryAsync<GameEventRow>(command);

                    gameEvents = rows
                        .Select(GameEventRow.ToGameEvent)
                        .ToArray();
                }

                await transaction.CommitAsync(cancellationToken);
            }

            return Tuple.Create(init, gameEvents);
        }

#pragma warning disable CS0649
        private sealed class get_game_init_by_id
        {
            public PlayerId first_player_id;
            public PlayerId player_id;
            public PlayerHandle player_handle;
            public PlayerType player_type;
            public BoardType board_type;
            public int num_sequences_to_win;
            public Seed seed;
        }
#pragma warning restore CS0649
    }
}
