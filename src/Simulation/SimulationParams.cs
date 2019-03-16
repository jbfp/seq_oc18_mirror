using System.Collections.Immutable;

namespace Sequence.Simulation
{
    public sealed class SimulationParams
    {
        public IImmutableList<Bot> Players { get; set; }
        public BoardType BoardType { get; set; }
        public PlayerHandle CreatedBy { get; set; }
        public bool RandomFirstPlayer { get; set; }
        public Seed Seed { get; set; }
        public int WinCondition { get; set; }
    }
}
