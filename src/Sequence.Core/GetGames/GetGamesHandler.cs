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
            PlayerId playerId,
            CancellationToken cancellationToken)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _provider.GetGamesForPlayerAsync(playerId, cancellationToken);
        }
    }
}
