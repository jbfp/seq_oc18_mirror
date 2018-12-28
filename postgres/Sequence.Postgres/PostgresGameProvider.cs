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
    public sealed class PostgresGameProvider : PostgresBase, IGameProvider
    {
        public PostgresGameProvider(IOptions<PostgresOptions> options) : base(options)
        {
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
            using (var transaction = connection.BeginTransaction())
            {
                {
                    get_game_init_by_id[] rows;

                    var commandText = @"
                        SELECT
                          gp0.player_id AS first_player_id
                        , gp1.player_id as player_id
                        , g.seed
                        FROM public.game AS g

                        INNER JOIN public.game_player AS gp0
                        ON gp0.id = g.first_player_id

                        INNER JOIN public.game_player AS gp1
                        ON gp1.game_id = g.id

                        WHERE g.game_id = @gameId;";

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
                        players: rows.Select(row => row.player_id).ToImmutableList(),
                        firstPlayer: rows[0].first_player_id,
                        seed: rows[0].seed
                    );
                }

                {
                    var commandText = @"
                        SELECT
                          idx
                        , gp0.player_id AS by_player_id
                        , card_drawn
                        , card_used
                        , chip
                        , coord
                        , gp1.player_id AS next_player_id
                        , sequence
                        FROM public.game_event AS ge
                        INNER JOIN public.game AS g ON g.id = ge.game_id
                        INNER JOIN public.game_player AS gp0 ON gp0.game_id = g.id AND gp0.id = ge.by_player_id
                        LEFT JOIN public.game_player AS gp1 ON gp1.game_id = g.id AND gp1.id = ge.next_player_id
                        WHERE g.game_id = @gameId;";

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
            public PlayerId player_id;
            public PlayerId first_player_id;
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
