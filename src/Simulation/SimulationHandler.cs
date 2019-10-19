using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Simulation
{
    public sealed class SimulationHandler
    {
        private readonly ISimulationStore _store;

        public SimulationHandler(ISimulationStore store)
        {
            _store = store;
        }

        public async Task<GameId> CreateSimulationAsync(
            SimulationParams parameters,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var players = parameters.Players;
            var random = parameters.Seed.ToRandom();
            var firstPlayerIdx = parameters.RandomFirstPlayer ? random.Next(players.Count) : 0;

            var newSimulation = new NewSimulation(
                boardType: parameters.BoardType,
                createdBy: parameters.CreatedBy,
                firstPlayerIndex: firstPlayerIdx,
                players: players,
                seed: parameters.Seed,
                winCondition: parameters.WinCondition);

            return await _store.SaveNewSimulationAsync(newSimulation, cancellationToken);
        }

        public Task<IImmutableList<GameId>> GetSimulationsAsync(
            PlayerHandle player,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _store.GetSimulationsAsync(player, cancellationToken);
        }
    }
}
