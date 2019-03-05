using System;

namespace Sequence.Bots
{
    public sealed class BotTask
    {
        public BotTask(GameId gameId, Player player)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public GameId GameId { get; }
        public Player Player { get; }
    }
}
