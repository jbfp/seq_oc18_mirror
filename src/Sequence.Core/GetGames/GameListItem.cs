using System;

namespace Sequence.Core.GetGames
{
    public sealed class GameListItem
    {
        public GameListItem(GameId gameId, PlayerId nextPlayerId, PlayerId opponent)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            NextPlayerId = nextPlayerId;
            Opponent = opponent ?? throw new ArgumentNullException(nameof(opponent));
        }

        public GameId GameId { get; }
        public PlayerId NextPlayerId { get; }
        public PlayerId Opponent { get; }
    }
}
