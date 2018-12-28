using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.GetGames
{
    public sealed class GetGamesHandler
    {
        private readonly IGameListProvider _provider;

        public GetGamesHandler(IGameListProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task<GameList> GetGamesForPlayerAsync(
            PlayerHandle player,
            CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _provider.GetGamesForPlayerAsync(player, cancellationToken);
        }
    }
}
