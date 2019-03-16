using System.Collections.Immutable;

namespace Sequence.Simulation
{
    public sealed class NewSimulation
    {
        public BoardType BoardType { get; set; }
        public PlayerHandle CreatedBy { get; set; }
        public int FirstPlayerIndex { get; set; }
        public IImmutableList<Bot> Players { get; set; }
        public Seed Seed { get; set; }
        public int WinCondition { get; set; }
    }
}
