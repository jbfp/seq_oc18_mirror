using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGame
{
    public interface IGameProvider
    {
        Task<GameState> GetGameStateByIdAsync(
            GameId gameId,
            CancellationToken cancellationToken);
    }
}
