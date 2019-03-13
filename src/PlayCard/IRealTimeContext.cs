using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sequence.PlayCard
{
    public interface IRealTimeContext
    {
        Task SendGameUpdatesAsync(PlayerId playerId, IEnumerable<GameUpdated> updates);
    }
}
