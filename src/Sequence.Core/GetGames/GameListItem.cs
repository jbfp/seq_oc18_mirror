using System;

namespace Sequence.Core.GetGames
{
    public sealed class GameListItem
    {
        public GameListItem(GameId gameId, PlayerId currentPlayer, PlayerId opponent)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            CurrentPlayer = currentPlayer;
            Opponent = opponent ?? throw new ArgumentNullException(nameof(opponent));
        }

        public GameId GameId { get; }
        public PlayerId CurrentPlayer { get; }
        public PlayerId Opponent { get; }
    }
}
