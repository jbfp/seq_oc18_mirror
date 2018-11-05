using System.Threading.Tasks;

namespace Sequence.Core
{
    public interface IGameUpdatedNotifier
    {
        Task SendAsync(GameId gameId, int version);
    }
}
