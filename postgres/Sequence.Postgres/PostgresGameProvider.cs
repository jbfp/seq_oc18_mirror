using Dapper;
using Microsoft.Extensions.Options;
using Sequence.Core;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresGameProvider : IGameProvider
    {
        private readonly NpgsqlConnectionFactory _connectionFactory;

        public PostgresGameProvider(NpgsqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
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
                        seed: rows[0].seed
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
                        , ge.sequence
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

                    var rows = await connection.QueryAsync<get_game_event_by_game_id>(command);

                    gameEvents = rows.Select(row => new GameEvent
                    {
                        ByPlayerId = row.by_player_id,
                        CardDrawn = row.card_drawn?.ToCard(),
                        CardUsed = row.card_used.ToCard(),
                        Chip = row.chip,
                        Coord = row.coord.ToCoord(),
                        Index = row.idx,
                        NextPlayerId = row.next_player_id,
                        Sequence = row.sequence?.ToSequence(),
                    }).ToArray();
                }

                await transaction.CommitAsync(cancellationToken);
            }

            return new Game(init, gameEvents);
        }

#pragma warning disable CS0649
        private sealed class get_game_init_by_id
        {
            public PlayerId first_player_id;
            public PlayerId player_id;
            public PlayerHandle player_handle;
            public PlayerType player_type;
            public Seed seed;
        }

        private sealed class get_game_event_by_game_id
        {
            public int idx;
            public PlayerId by_player_id;
            public CardComposite card_drawn;
            public CardComposite card_used;
            public Team? chip;
            public CoordComposite coord;
            public PlayerId next_player_id;
            public SequenceComposite sequence;
        }
#pragma warning restore CS0649
    }
}
