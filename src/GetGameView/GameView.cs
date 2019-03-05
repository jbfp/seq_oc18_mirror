using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.GetGameView
{
    public sealed class GameView
    {
        private GameView(GameState state, int playerIdx)
        {
            Chips = state.Chips.Select(kvp => new ChipView
            {
                Coord = kvp.Key,
                IsLocked = state.CoordsInSequence.Contains(kvp.Key),
                Team = kvp.Value,
            }).ToImmutableList();
            CurrentPlayerId = state.CurrentPlayerId;
            Discards = state.Discards;
            Moves = state.GameEvents.Select(gameEvent => new MoveView
            {
                ByPlayerId = gameEvent.ByPlayerId,
                CardUsed = gameEvent.CardUsed,
                Coord = gameEvent.Coord,
                Index = gameEvent.Index,
            }).ToImmutableList();
            NumberOfCardsInDeck = state.Deck.Count;
            Players = Enumerable.Range(0, state.NumberOfPlayers).Select(idx => new PlayerView
            {
                Handle = state.PlayerHandleByIdx[idx],
                Id = state.PlayerIdByIdx[idx],
                NumberOfCards = state.PlayerHandByIdx[idx].Count,
                Team = state.PlayerTeamByIdx[idx],
                Type = state.PlayerTypeByIdx[idx],
            }).ToImmutableList();
            Rules = new RulesView
            {
                BoardType = FromIBoardType(state.BoardType),
                WinCondition = state.WinCondition,
            };
            Version = state.Version;
            Winner = state.Winner;

            if (playerIdx >= 0 && playerIdx < state.NumberOfPlayers)
            {
                Hand = state.PlayerHandByIdx[playerIdx];
                PlayerHandle = state.PlayerHandleByIdx[playerIdx];
                PlayerId = state.PlayerIdByIdx[playerIdx];
                Team = state.PlayerTeamByIdx[playerIdx];
            }
        }

        public IImmutableList<ChipView> Chips { get; }
        public PlayerId CurrentPlayerId { get; }
        public IImmutableStack<Card> Discards { get; }
        public IImmutableList<Card> Hand { get; }
        public IImmutableList<MoveView> Moves { get; }
        public int NumberOfCardsInDeck { get; }
        public PlayerHandle PlayerHandle { get; }
        public PlayerId PlayerId { get; }
        public IImmutableList<PlayerView> Players { get; }
        public RulesView Rules { get; }
        public Team? Team { get; }
        public int Version { get; }
        public Team? Winner { get; }

        public static GameView FromGameState(GameState state, PlayerId playerId)
        {
            return new GameView(state, state.PlayerIdByIdx.IndexOf(playerId));
        }

        public static GameView FromGameState(GameState state, PlayerHandle playerHandle)
        {
            return new GameView(state, state.PlayerHandleByIdx.IndexOf(playerHandle));
        }

        private static BoardType FromIBoardType(IBoardType boardType)
        {
            switch (boardType)
            {
                case OneEyedJackBoard b1:
                    return BoardType.OneEyedJack;

                case SequenceBoard b2:
                    return BoardType.Sequence;

                default:
                    throw new ArgumentOutOfRangeException(nameof(boardType), boardType.GetType(), null);
            }
        }
    }

    public sealed class ChipView
    {
        public Coord Coord { get; set; }
        public bool IsLocked { get; set; }
        public Team Team { get; set; }
    }

    public sealed class MoveView
    {
        public PlayerId ByPlayerId { get; set; }
        public Card CardUsed { get; set; }
        public Coord Coord { get; set; }
        public int Index { get; set; }
    }

    public sealed class PlayerView
    {
        public PlayerHandle Handle { get; set; }
        public PlayerId Id { get; set; }
        public int NumberOfCards { get; set; }
        public Team Team { get; set; }
        public PlayerType Type { get; set; }
    }

    public sealed class RulesView
    {
        public BoardType BoardType { get; set; }
        public int WinCondition { get; set; }
    }
}
