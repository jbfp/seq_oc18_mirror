using System;
using System.Collections.Immutable;

namespace Sequence.GetGameList
{
    public sealed class GameList
    {
        public GameList(IImmutableList<GameListItem> games)
        {
            Games = games ?? throw new ArgumentNullException(nameof(games));
        }

        public IImmutableList<GameListItem> Games { get; }
    }
}
