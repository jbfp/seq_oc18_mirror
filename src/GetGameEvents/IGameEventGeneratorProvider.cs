using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameEvents
{
    public interface IGameEventGeneratorProvider
    {
        Task<GameEventGenerator> GetGameEventGeneratorByIdAsync(
            GameId gameId,
            CancellationToken cancellationToken);
    }
}
