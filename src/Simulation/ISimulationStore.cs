using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Simulation
{
    public interface ISimulationStore
    {
        Task<IImmutableList<GameId>> GetSimulationsAsync(
            PlayerHandle player,
            CancellationToken cancellationToken);

        Task<GameId> SaveNewSimulationAsync(
            NewSimulation newSimulation,
            CancellationToken cancellationToken);
    }
}
