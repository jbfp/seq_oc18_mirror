using System.Collections.Generic;
using System;

namespace Sequence.Core.GetGames
{
    public sealed class GameList
    {
        public GameList(IReadOnlyList<GameListItem> games)
        {
            Games = games ?? throw new ArgumentNullException(nameof(games));
        }

        public IReadOnlyList<GameListItem> Games { get; }
    }
}
