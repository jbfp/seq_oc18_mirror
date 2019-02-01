using System;
using System.Collections.Immutable;

namespace Sequence.Core.GetGames
{
    public sealed class GameListItem
    {
        public GameListItem(GameId gameId, PlayerHandle currentPlayer, IImmutableList<PlayerHandle> opponents, DateTimeOffset? lastMoveAt)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            CurrentPlayer = currentPlayer;
            Opponents = opponents ?? throw new ArgumentNullException(nameof(opponents));
            LastMoveAt = lastMoveAt;
        }

        public GameId GameId { get; }
        public PlayerHandle CurrentPlayer { get; }
        public IImmutableList<PlayerHandle> Opponents { get; }
        public DateTimeOffset? LastMoveAt { get; }
    }
}
