using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.Play
{
    public interface IGameEventStore
    {
        Task AddEventAsync(GameId gameId, GameEvent gameEvent, CancellationToken cancellationToken);
    }
}
