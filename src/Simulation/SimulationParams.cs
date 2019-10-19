using System.Collections.Immutable;

namespace Sequence.Simulation
{
    public sealed class SimulationParams
    {
        public SimulationParams(
            BoardType boardType,
            PlayerHandle createdBy,
            IImmutableList<Bot> players,
            bool randomFirstPlayer,
            Seed seed,
            int winCondition)
        {
            BoardType = boardType;
            CreatedBy = createdBy;
            Players = players;
            RandomFirstPlayer = randomFirstPlayer;
            Seed = seed;
            WinCondition = winCondition;
        }

        public BoardType BoardType { get; }
        public PlayerHandle CreatedBy { get; }
        public IImmutableList<Bot> Players { get; }
        public bool RandomFirstPlayer { get; }
        public Seed Seed { get; }
        public int WinCondition { get; }
    }
}
