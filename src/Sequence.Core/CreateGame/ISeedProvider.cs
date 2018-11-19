using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.CreateGame
{
    public interface ISeedProvider
    {
        Task<Seed> GenerateSeedAsync(CancellationToken cancellationToken);
    }
}
