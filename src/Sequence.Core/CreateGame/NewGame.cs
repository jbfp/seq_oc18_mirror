using System;

namespace Sequence.Core.CreateGame
{
    public sealed class NewGame
    {
        public NewGame(PlayerId player1, PlayerId player2, PlayerId firstPlayerId, Seed seed)
        {
            Player1 = player1 ?? throw new ArgumentNullException(nameof(player1));
            Player2 = player2 ?? throw new ArgumentNullException(nameof(player2));
            FirstPlayerId = firstPlayerId ?? throw new ArgumentNullException(nameof(firstPlayerId));
            Seed = seed;

            if (Player1.Equals(Player2))
            {
                throw new ArgumentException("Player 1 and Player 2 must not be the same player.");
            }
        }

        public PlayerId Player1 { get; }
        public PlayerId Player2 { get; }
        public PlayerId FirstPlayerId { get; }
        public Seed Seed { get; }
    }
}
