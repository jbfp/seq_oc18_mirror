using System.Collections.Immutable;
using Xunit;

namespace Sequence.Test
{
    public sealed class GameState_ExchangeDeadCardTest
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
        public void ExchangeDeadCardSetsFlag()
        {
            var sut = _default;

            sut = _default.Apply(new GameEvent
            {
                ByPlayerId = sut.PlayerIdByIdx[0],
                CardUsed = sut.PlayerHandByIdx[0][0],
                Coord = new Coord(-1, -1), // Exchange dead card marker.
            });

            Assert.True(sut.HasExchangedDeadCard);
        }
    }
}
