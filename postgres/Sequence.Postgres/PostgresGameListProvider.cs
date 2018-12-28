using Dapper;
using Microsoft.Extensions.Options;
using Sequence.Core;
using Sequence.Core.GetGames;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresGameListProvider : PostgresBase, IGameListProvider
    {
        private readonly IOptions<PostgresOptions> _options;

        public PostgresGameListProvider(IOptions<PostgresOptions> options) : base(options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<GameList> GetGamesForPlayerAsync(PlayerHandle player, CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<GameListItem> gameListItems;

            using (var connection = await CreateAndOpenAsync(cancellationToken))
            {
                var command = new CommandDefinition(
                    commandText: "SELECT * FROM public.get_game_list_for_player(@player);",
                    parameters: new { player },
                    cancellationToken: cancellationToken
                );

                var rows = await connection.QueryAsync<get_game_list_for_player>(command);

                gameListItems = rows
                    .Select(get_game_list_for_player.ToGameListItem)
                    .ToList()
                    .AsReadOnly();
            }

            return new GameList(gameListItems);
        }

#pragma warning disable CS0649
        private sealed class get_game_list_for_player
        {
            public GameId game_id;
            public PlayerHandle next_player_id;
            public string[] opponents;

            public static GameListItem ToGameListItem(get_game_list_for_player row)
            {
                return new GameListItem(
                    row.game_id,
                    row.next_player_id,
                    row.opponents.Select(o => new PlayerHandle(o)).ToImmutableList()
                );
            }
        }
#pragma warning restore CS0649
    }
}
