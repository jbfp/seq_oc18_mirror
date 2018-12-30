using System;

namespace Sequence.Core.Bots
{
    public sealed class BotTask
    {
        public BotTask(GameId gameId, PlayerHandle playerHandle)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            PlayerHandle = playerHandle ?? throw new ArgumentNullException(nameof(playerHandle));
        }

        public GameId GameId { get; }
        public PlayerHandle PlayerHandle { get; }
    }
}
