using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
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

namespace Sequence.Postgres
{
    public sealed class PostgresAdapter : IGameEventStore, IGameProvider, IGameListProvider, IGameStore
    {
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
                    var commandText = "SELECT id FROM game WHERE game_id = CAST(@gameId AS UUID);";

                    var parameters = new { gameId = gameId.ToString() };

                    var command = new CommandDefinition(
                        commandText,
                        parameters,
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    actualGameId = await connection.QuerySingleAsync<int>(command);
                }

                {
                    string commandText;

                    if (gameEvent.CardDrawn == null)
                    {
                        commandText = @"
                            INSERT INTO
                                game_event (game_id, idx, by_player_id, card_used, chip, coord, next_player_id)
                            VALUES
                                (@gameId, @idx, @byPlayerId, CAST(ROW(@cardUsedDeckNo, @cardUsedSuit, @cardUsedRank) AS card), CAST(@chip AS chip), CAST(ROW(@coordCol, @coordRow) AS coord), @nextPlayerId);";
                    }
                    else
                    {
                        commandText = @"
                            INSERT INTO
                                game_event (game_id, idx, by_player_id, card_drawn, card_used, chip, coord, next_player_id)
                            VALUES
                                (@gameId, @idx, @byPlayerId, CAST(ROW(@cardDrawnDeckNo, @cardDrawnSuit, @cardDrawnRank) AS card), CAST(ROW(@cardUsedDeckNo, @cardUsedSuit, @cardUsedRank) AS card), CAST(@chip AS chip), CAST(ROW(@coordCol, @coordRow) AS coord), @nextPlayerId);";
                    }

                    var parameters = new
                    {
                        gameId = actualGameId,
                        idx = gameEvent.Index,
                        byPlayerId = gameEvent.ByPlayerId.ToString(),
                        cardDrawnDeckNo = gameEvent.CardDrawn?.DeckNo.ToString().ToLowerInvariant(),
                        cardDrawnSuit = gameEvent.CardDrawn?.Suit.ToString().ToLowerInvariant(),
                        cardDrawnRank = gameEvent.CardDrawn?.Rank.ToString().ToLowerInvariant(),
                        cardUsedDeckNo = gameEvent.CardUsed.DeckNo.ToString().ToLowerInvariant(),
                        cardUsedSuit = gameEvent.CardUsed.Suit.ToString().ToLowerInvariant(),
                        cardUsedRank = gameEvent.CardUsed.Rank.ToString().ToLowerInvariant(),
                        chip = gameEvent.Chip?.ToString().ToLowerInvariant(),
                        coordCol = gameEvent.Coord.Column,
                        coordRow = gameEvent.Coord.Row,
                        nextPlayerId = gameEvent.NextPlayerId?.ToString(),
                    };

                    var command = new CommandDefinition(
                        commandText,
                        parameters,
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    await connection.ExecuteAsync(command);
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
                        commandText: "SELECT player1, player2, first_player_id, seed FROM game WHERE game_id = CAST(@gameId AS UUID);",
                        parameters: new { gameId = gameId.ToString() },
                        transaction,
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
                        new PlayerId((string)result.first_player_id),
                        new Seed((int)result.seed)
                    );
                }

                {
                    var commandText = @"
                        SELECT
                          idx
                        , by_player_id
                        , card_drawn::TEXT
                        , card_used::TEXT
                        , chip::TEXT
                        , coord::TEXT
                        , next_player_id
                        FROM game_event AS ge
                        INNER JOIN game AS g ON g.id = ge.game_id
                        WHERE g.game_id = CAST(@gameId AS UUID);";

                    var parameters = new { gameId = gameId.ToString() };

                    var command = new CommandDefinition(
                        commandText,
                        parameters,
                        transaction,
                        cancellationToken: cancellationToken
                    );

                    var rows = await connection.QueryAsync(command);

                    Card MapTextToCard(string value)
                    {
                        var trim = value.Trim('(', ')');
                        var split = trim.Split(',');
                        var deckNo = (DeckNo)Enum.Parse(typeof(DeckNo), split[0], ignoreCase: true);
                        var suit = (Suit)Enum.Parse(typeof(Suit), split[1], ignoreCase: true);
                        var rank = (Rank)Enum.Parse(typeof(Rank), split[2], ignoreCase: true);
                        return new Card(deckNo, suit, rank);
                    }

                    Coord MapTextToCoord(string value)
                    {
                        var trim = value.Trim('(', ')');
                        var split = trim.Split(',');
                        var column = int.Parse(split[0]);
                        var row = int.Parse(split[1]);
                        return new Coord(column, row);
                    }

                    gameEvents = rows.Select(row => new GameEvent
                    {
                        ByPlayerId = new PlayerId((string)row.by_player_id),
                        CardDrawn = row.card_drawn == null ? null : MapTextToCard(row.card_drawn),
                        CardUsed = MapTextToCard(row.card_used),
                        Chip = row.chip == null ? (Team?)null : (Team)Enum.Parse(typeof(Team), row.chip, ignoreCase: true),
                        Coord = MapTextToCoord(row.coord),
                        Index = (int)row.idx,
                        NextPlayerId = new PlayerId((string)row.next_player_id),
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
                    parameters: new { playerId = playerId.ToString() },
                    cancellationToken: cancellationToken
                );

                var rows = await connection.QueryAsync<get_game_list_for_player>(command);

                GameListItem MapRowToGameListItem(get_game_list_for_player row)
                {
                    var gameId = new GameId(row.game_id);
                    var nextPlayerId = row.next_player_id == null ? null : new PlayerId(row.next_player_id);
                    var opponent = new PlayerId(row.opponent);
                    return new GameListItem(gameId, nextPlayerId, opponent);
                }

                gameListItems = rows
                    .Select(MapRowToGameListItem)
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

            Guid gameId;

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
                    gameId,
                    player1 = newGame.Player1.ToString(),
                    player2 = newGame.Player2.ToString(),
                    firstPlayerId = newGame.FirstPlayerId.ToString(),
                    seed = newGame.Seed.ToInt32(),
                    version = 1,
                };

                var command = new CommandDefinition(
                    commandText,
                    parameters,
                    cancellationToken: cancellationToken
                );

                gameId = await connection.QuerySingleAsync<Guid>(command);
            }

            return new GameId(gameId);
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
            public Guid game_id;
            public string next_player_id;
            public string opponent;
        }
#pragma warning restore CS0649
    }
}
