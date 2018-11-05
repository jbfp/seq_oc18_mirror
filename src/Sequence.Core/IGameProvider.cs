using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core
{
    public interface IGameProvider
    {
        Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken);
    }
}
