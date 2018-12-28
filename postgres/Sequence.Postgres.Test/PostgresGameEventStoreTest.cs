using Sequence.Core;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Postgres.Test
{
    [Collection(PostgresDockerContainerCollection.Name)]
    public sealed class PostgresGameEventStoreTest : PostgresTestBase
    {
        public PostgresGameEventStoreTest(PostgresDockerContainerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CanAddGameEvent()
        {
            await TestAddEventAsync(new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = null,
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = new PlayerId("player 2"),
            });
        }

        [Fact]
        public async Task CanAddGameEventWithoutNextPlayer()
        {
            await TestAddEventAsync(new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = null,
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = null,
            });
        }

        [Fact]
        public async Task CanAddGameEventWithCardDrawn()
        {
            await TestAddEventAsync(new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = new Card(DeckNo.One, Suit.Spades, Rank.Five),
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = new PlayerId("player 2"),
            });
        }

        [Fact]
        public async Task CanAddGameEventWithSequence()
        {
            await TestAddEventAsync(new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = new Card(DeckNo.One, Suit.Spades, Rank.Five),
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = null,
                Sequence = new Seq(Team.Blue, ImmutableList.Create(
                    new Coord(4, 2),
                    new Coord(5, 2),
                    new Coord(6, 2),
                    new Coord(7, 2),
                    new Coord(8, 2)
                )),
            });
        }

        private async Task TestAddEventAsync(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                throw new System.ArgumentNullException(nameof(gameEvent));
            }

            var options = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(options);
            var sut = new PostgresGameEventStore(options);
            await sut.AddEventAsync(gameId, gameEvent, CancellationToken.None);
        }
    }
}
