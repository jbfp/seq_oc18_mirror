using System.Collections.Immutable;
using Xunit;

namespace Sequence.Test
{
    public sealed class GameState_DeadCard_Test
    {
        private static readonly GameState _default = new GameState(
            new GameInit(
                players: ImmutableArray.Create(
                    new Player(
                        new PlayerId(1),
                        new PlayerHandle("test 1")
                    ),
                    new Player(
                        new PlayerId(2),
                        new PlayerHandle("test 2")
                    )
                ),
                firstPlayerId: new PlayerId(1),
                seed: new Seed(42),
                boardType: BoardType.OneEyedJack,
                numSequencesToWin: 2
            )
        );

        [Fact]
        public void DeadCard1()
        {
            var sut = _default;
            var card = sut.PlayerHandByIdx[0][0];
            var tile = new Tile(card.Suit, card.Rank);
            var (coord0, coord1) = sut.BoardType.CoordsByTile[tile];

            // Occupy coord0, coord1 with any two cards to make 'card' dead.
            sut = _default.Apply(new GameEvent
            {
                ByPlayerId = sut.PlayerIdByIdx[0],
                CardUsed = sut.PlayerHandByIdx[0][1],
                Chip = sut.PlayerTeamByIdx[0],
                Coord = coord0,
            }).Apply(new GameEvent
            {
                ByPlayerId = sut.PlayerIdByIdx[0],
                CardUsed = sut.PlayerHandByIdx[0][1],
                Chip = sut.PlayerTeamByIdx[0],
                Coord = coord1,
            });

            Assert.True(sut.DeadCards.Contains(card));
        }

        [Fact]
        public void DeadCardResurrection()
        {
            var sut = _default;
            var card = sut.PlayerHandByIdx[0][0];
            var tile = new Tile(card.Suit, card.Rank);
            var (coord0, coord1) = sut.BoardType.CoordsByTile[tile];

            // Occupy coord0, coord1 with any two cards to make 'card' dead.
            sut = _default.Apply(new GameEvent
            {
                ByPlayerId = sut.PlayerIdByIdx[0],
                CardUsed = sut.PlayerHandByIdx[0][1],
                Chip = sut.PlayerTeamByIdx[0],
                Coord = coord0,
            }).Apply(new GameEvent
            {
                ByPlayerId = sut.PlayerIdByIdx[0],
                CardUsed = sut.PlayerHandByIdx[0][1],
                Chip = sut.PlayerTeamByIdx[0],
                Coord = coord1,
            });

            Assert.True(sut.DeadCards.Contains(card));

            // Remove chip from coord1 to make 'card' alive again.
            sut = sut.Apply(new GameEvent
            {
                ByPlayerId = sut.PlayerIdByIdx[0],
                CardUsed = sut.PlayerHandByIdx[0][1],
                Chip = null,
                Coord = coord1,
            });

            Assert.False(sut.DeadCards.Contains(card));
        }
    }
}
