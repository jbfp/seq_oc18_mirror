using System;
using System.Collections.Immutable;

namespace Sequence.GetGame
{
    public sealed class InitialGameState
    {
        public InitialGameState(
            BoardType boardType,
            PlayerId firstPlayerId,
            IImmutableList<Card> hand,
            int numCardsInDeck,
            PlayerHandle playerHandle,
            PlayerId playerId,
            IImmutableList<Player> players,
            Team team,
            int winCondition)
        {
            BoardType = boardType;
            FirstPlayerId = firstPlayerId ?? throw new ArgumentNullException(nameof(firstPlayerId));
            Hand = hand;
            NumCardsInDeck = numCardsInDeck;
            PlayerHandle = playerHandle;
            PlayerId = playerId;
            Players = players ?? throw new ArgumentNullException(nameof(players));
            Team = team;
            WinCondition = winCondition;
        }

        public BoardType BoardType { get; }
        public PlayerId FirstPlayerId { get; }
        public IImmutableList<Card> Hand { get; }
        public int NumCardsInDeck { get; }
        public PlayerHandle PlayerHandle { get; }
        public PlayerId PlayerId { get; }
        public IImmutableList<Player> Players { get; }
        public Team Team { get; }
        public int WinCondition { get; }
    }
}
