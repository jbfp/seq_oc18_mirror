using System;

namespace Sequence.Core.CreateGame
{
    public sealed class NewGame
    {
        public NewGame(PlayerList players, Seed seed)
        {
            PlayerList = players ?? throw new ArgumentNullException(nameof(players));
            Seed = seed;
        }

        public PlayerList PlayerList { get; }
        public Seed Seed { get; }
    }
}
