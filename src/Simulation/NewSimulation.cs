using System.Collections.Immutable;

namespace Sequence.Simulation
{
    public sealed class NewSimulation
    {
        public NewSimulation(
            BoardType boardType,
            PlayerHandle createdBy,
            int firstPlayerIndex,
            IImmutableList<Bot> players,
            Seed seed,
            int winCondition)
        {
            BoardType = boardType;
            CreatedBy = createdBy;
            FirstPlayerIndex = firstPlayerIndex;
            Players = players;
            Seed = seed;
            WinCondition = winCondition;
        }

        public BoardType BoardType { get; }
        public PlayerHandle CreatedBy { get; }
        public int FirstPlayerIndex { get; }
        public IImmutableList<Bot> Players { get; }
        public Seed Seed { get; }
        public int WinCondition { get; }
    }
}
