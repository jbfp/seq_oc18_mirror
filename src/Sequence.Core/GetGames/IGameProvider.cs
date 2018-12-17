using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.GetGames
{
    public interface IGameListProvider
    {
        Task<GameList> GetGamesForPlayerAsync(
            PlayerId playerId,
            CancellationToken cancellationToken);
    }
}
