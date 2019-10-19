using Sequence.Postgres;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Test.Postgres
{
    [Trait("Category", "Integration")]
    [Collection(PostgresDockerContainerCollection.Name)]
    public sealed class PostgresGameStateProviderTest : PostgresTestBase
    {
        public PostgresGameStateProviderTest(PostgresDockerContainerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CanGetGame()
        {
            // Given:
            var db = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(db);
            var sut = new PostgresGameStateProvider(
                new PostgresGameProvider(db));

            // Then:
            var game = await sut.GetGameByIdAsync(gameId, CancellationToken.None);

            // Then:
            Assert.NotNull(game);
        }

        [Fact]
        public async Task CanGetGameWithOneEvent()
        {
            // Given:
            var db = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(db);

            await AddEventAsync(db, gameId, new GameEvent(
                byPlayerId: new PlayerId(1),
                cardDrawn: null,
                cardUsed: new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                chip: Team.Green,
                coord: new Coord(4, 2),
                index: 2,
                nextPlayerId: new PlayerId(2),
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            var sut = new PostgresGameStateProvider(
                new PostgresGameProvider(db));

            // Then:
            var game = await sut.GetGameByIdAsync(gameId, CancellationToken.None);

            // Then:
            Assert.NotNull(game);
        }

        [Fact]
        public async Task CanGetGameWithOneEventWithCardDrawn()
        {
            // Given:
            var db = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(db);

            await AddEventAsync(db, gameId, new GameEvent(
                byPlayerId: new PlayerId(1),
                cardDrawn: new Card(DeckNo.One, Suit.Spades, Rank.Five),
                cardUsed: new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                chip: Team.Green,
                coord: new Coord(4, 2),
                index: 2,
                nextPlayerId: new PlayerId(2),
                sequences: ImmutableArray<Seq>.Empty,
                winner: null));

            var sut = new PostgresGameStateProvider(
                new PostgresGameProvider(db));

            // Then:
            var game = await sut.GetGameByIdAsync(gameId, CancellationToken.None);

            // Then:
            Assert.NotNull(game);
        }

        [Fact]
        public async Task CanGetGameWithOneEventWithSequence()
        {
            // Given:
            var db = await CreateDatabaseAsync();
            var gameId = await CreateGameAsync(db);

            await AddEventAsync(db, gameId, new GameEvent(
                byPlayerId: new PlayerId(1),
                cardDrawn: new Card(DeckNo.One, Suit.Spades, Rank.Five),
                cardUsed: new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                chip: Team.Green,
                coord: new Coord(4, 2),
                index: 2,
                nextPlayerId: new PlayerId(2),
                sequences: ImmutableArray.Create(
                    new Seq(Team.Blue, ImmutableList.Create(
                        new Coord(4, 2),
                        new Coord(5, 2),
                        new Coord(6, 2),
                        new Coord(7, 2),
                        new Coord(8, 2)))),
                winner: null));

            var sut = new PostgresGameStateProvider(
                new PostgresGameProvider(db));

            // Then:
            var game = await sut.GetGameByIdAsync(gameId, CancellationToken.None);

            // Then:
            Assert.NotNull(game);
        }
    }
}
