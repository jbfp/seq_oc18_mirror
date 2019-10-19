namespace Sequence.Bots
{
    public sealed class BotTask
    {
        public BotTask(GameId gameId, Player player)
        {
            GameId = gameId;
            Player = player;
        }

        public GameId GameId { get; }
        public Player Player { get; }
    }
}
