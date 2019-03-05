using System.Threading;
using System.Threading.Tasks;

namespace Sequence
{
    public interface IGameStateProvider
    {
        Task<GameState> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken);
    }
}
