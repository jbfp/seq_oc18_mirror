using Sequence.CreateGame;
using System;

namespace Sequence.Test.CreateGame
{
    internal static class TestPlayer
    {
        public static NewPlayer Get
        {
            get
            {
                var id = Guid.NewGuid();
                var handle = new PlayerHandle(id.ToString());
                return new NewPlayer(handle, PlayerType.User);
            }
        }
    }
}
