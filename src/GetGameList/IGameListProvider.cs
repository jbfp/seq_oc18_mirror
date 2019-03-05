using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameList
{
    public interface IGameListProvider
    {
        Task<GameList> GetGamesForPlayerAsync(
            PlayerHandle player,
            CancellationToken cancellationToken);
    }
}
