using System;

namespace Sequence.Core.Test
{
    internal static class GameIdGenerator
    {
        public static GameId Generate() => new GameId(Guid.NewGuid());
    }
}
