using System;

namespace Sequence.Core.GetGames
{
    public sealed class GameListItem
    {
        public GameListItem(GameId gameId, PlayerId nextPlayerId)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            NextPlayerId = nextPlayerId;
        }

        public GameId GameId { get; }
        public PlayerId NextPlayerId { get; }
    }
}
