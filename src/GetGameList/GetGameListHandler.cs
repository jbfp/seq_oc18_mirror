using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameList
{
    public sealed class GetGameListHandler
    {
        private readonly IGameListProvider _provider;

        public GetGameListHandler(IGameListProvider provider)
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
