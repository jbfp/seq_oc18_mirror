using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;
using Sequence.Core.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresAdapter : IGameEventStore, IGameProvider, IGameListProvider, IGameStore
    {
        static PostgresAdapter()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<DeckNo>("deckno");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Rank>("rank");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Suit>("suit");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Team>("chip");

            NpgsqlConnection.GlobalTypeMapper.MapComposite<CardComposite>("card");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<CoordComposite>("coord");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<SequenceComposite>("sequence");

            SqlMapper.AddTypeHandler<GameId>(new GameIdTypeHandler());
            SqlMapper.AddTypeHandler<PlayerId>(new PlayerIdTypeHandler());
            SqlMapper.AddTypeHandler<Seed>(new SeedTypeHandler());
        }

        private readonly IOptions<PostgresOptions> _options;
        private readonly ILogger _logger;

        public PostgresAdapter(IOptions<PostgresOptions> options, ILogger<PostgresAdapter> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                int actualGameId;

                {
                    var command = new CommandDefinition(
                        commandText: "SELECT id FROM game WHERE game_id = @gameId;",
                        parameters: new { gameId },
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    actualGameId = await connection.QuerySingleAsync<int>(command);
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
                    command.Parameters.AddWithValue("@gameId", actualGameId);
                    command.Parameters.AddWithValue("@idx", gameEvent.Index);
                    command.Parameters.AddWithValue("@byPlayerId", gameEvent.ByPlayerId.ToString());
                    command.Parameters.AddWithValue("@cardDrawn", (object)CardComposite.FromCard(gameEvent.CardDrawn) ?? DBNull.Value);
                    command.Parameters.AddWithValue("@cardUsed", CardComposite.FromCard(gameEvent.CardUsed));
                    command.Parameters.AddWithValue("@chip", (object)gameEvent.Chip ?? DBNull.Value);
                    command.Parameters.AddWithValue("@coord", CoordComposite.FromCoord(gameEvent.Coord));
                    command.Parameters.AddWithValue("@nextPlayerId", (object)gameEvent.NextPlayerId?.ToString() ?? DBNull.Value);
                    command.Parameters.AddWithValue("@sequence", (object)SequenceComposite.FromSequence(gameEvent.Sequence) ?? DBNull.Value);
                    command.Transaction = transaction;

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
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
            using (var transaction = connection.BeginTransaction())
            {
                {
                    var command = new CommandDefinition(
                        commandText: "SELECT player1, player2, first_player_id, seed FROM game WHERE game_id = @gameId;",
                        parameters: new { gameId },
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    var result = await connection.QuerySingleOrDefaultAsync<get_game_init_by_id>(command);

                    if (result == null)
                    {
                        return null;
                    }

                    init = new GameInit(result.player1, result.player2, result.first_player_id, result.seed);
                }

                {
                    var commandText = @"
                        SELECT
                          idx
                        , by_player_id
                        , card_drawn
                        , card_used
                        , chip
                        , coord
                        , next_player_id
                        , sequence
                        FROM game_event AS ge
                        INNER JOIN game AS g ON g.id = ge.game_id
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

        public async Task<GameList> GetGamesForPlayerAsync(PlayerId playerId, CancellationToken cancellationToken)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<GameListItem> gameListItems;

            using (var connection = await CreateAndOpenAsync(cancellationToken))
            {
                var command = new CommandDefinition(
                    commandText: "SELECT game_id, next_player_id, opponent FROM public.get_game_list_for_player(@playerId);",
                    parameters: new { playerId },
                    cancellationToken: cancellationToken
                );

                var rows = await connection.QueryAsync<get_game_list_for_player>(command);

                gameListItems = rows
                    .Select(row => new GameListItem(row.game_id, row.next_player_id, row.opponent))
                    .ToList()
                    .AsReadOnly();
            }

            return new GameList(gameListItems);
        }

        public async Task<GameId> PersistNewGameAsync(NewGame newGame, CancellationToken cancellationToken)
        {
            if (newGame == null)
            {
                throw new ArgumentNullException(nameof(newGame));
            }

            cancellationToken.ThrowIfCancellationRequested();

            GameId gameId;

            using (var connection = await CreateAndOpenAsync(cancellationToken))
            {
                var commandText = @"
                    INSERT INTO
                        game (player1, player2, first_player_id, seed, version)
                    VALUES
                        (@player1, @player2, @firstPlayerId, @seed, @version)
                    RETURNING game_id;";

                var parameters = new
                {
                    player1 = newGame.Player1,
                    player2 = newGame.Player2,
                    firstPlayerId = newGame.FirstPlayerId,
                    seed = newGame.Seed,
                    version = 1,
                };

                var command = new CommandDefinition(
                    commandText,
                    parameters,
                    cancellationToken: cancellationToken
                );

                gameId = await connection.QuerySingleAsync<GameId>(command);
            }

            return gameId;
        }

        private async Task<NpgsqlConnection> CreateAndOpenAsync(CancellationToken cancellationToken)
        {
            var connectionString = _options.Value.ConnectionString;
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

#pragma warning disable CS0649
        private sealed class get_game_list_for_player
        {
            public GameId game_id;
            public PlayerId next_player_id;
            public PlayerId opponent;
        }

        private sealed class get_game_init_by_id
        {
            public PlayerId player1;
            public PlayerId player2;
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
