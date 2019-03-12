using System.Threading.Tasks;

namespace Sequence.RealTime
{
    public interface IGameHubClient
    {
        Task UpdateGame(GameUpdated gameUpdated);
    }
}
