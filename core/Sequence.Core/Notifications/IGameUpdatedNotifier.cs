using System.Threading.Tasks;

namespace Sequence.Core.Notifications
{
    public interface IGameUpdatedNotifier
    {
        Task SendAsync(GameId gameId, GameEvent gameEvent);
    }
}
