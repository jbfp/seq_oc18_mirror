using System;
using System.Collections.Immutable;

namespace Sequence.Core.GetGames
{
    public sealed class GameListItem
    {
        public GameListItem(GameId gameId, PlayerId currentPlayer, IImmutableList<PlayerId> opponents)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            CurrentPlayer = currentPlayer;
            Opponents = opponents ?? throw new ArgumentNullException(nameof(opponents));
        }

        public GameId GameId { get; }
        public PlayerId CurrentPlayer { get; }
        public IImmutableList<PlayerId> Opponents { get; }
    }
}
