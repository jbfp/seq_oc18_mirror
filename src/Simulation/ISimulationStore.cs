using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Simulation
{
    public interface ISimulationStore
    {
        Task<GameId> SaveNewSimulationAsync(
            NewSimulation newSimulation,
            CancellationToken cancellationToken);
    }
}
