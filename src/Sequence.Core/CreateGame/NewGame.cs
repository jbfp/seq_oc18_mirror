using System;

namespace Sequence.Core.CreateGame
{
    public sealed class NewGame
    {
        public NewGame(PlayerId player1, PlayerId player2, Seed seed)
        {
            Player1 = player1 ?? throw new ArgumentNullException(nameof(player1));
            Player2 = player2 ?? throw new ArgumentNullException(nameof(player2));
            Seed = seed;
        }

        public PlayerId Player1 { get; }
        public PlayerId Player2 { get; }
        public Seed Seed { get; }
    }
}
