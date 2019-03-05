using System;

namespace Sequence.Test
{
    internal static class GameIdGenerator
    {
        public static GameId Generate() => new GameId(Guid.NewGuid());
    }
}
