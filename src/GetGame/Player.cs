namespace Sequence.GetGame
{
    public sealed class Player
    {
        public Player(
            PlayerHandle handle,
            PlayerId id,
            int numberOfCards,
            Team team,
            PlayerType type)
        {
            Handle = handle;
            Id = id;
            NumberOfCards = numberOfCards;
            Team = team;
            Type = type;
        }

        public PlayerHandle Handle { get; }
        public PlayerId Id { get; }
        public int NumberOfCards { get; }
        public Team Team { get; }
        public PlayerType Type { get; }
    }
}
