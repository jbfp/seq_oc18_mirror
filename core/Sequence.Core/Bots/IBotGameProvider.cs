using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.Bots
{
    public interface IBotGameProvider
    {
        Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken);
    }
}
