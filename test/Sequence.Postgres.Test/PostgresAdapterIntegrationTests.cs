using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Postgres.Test
{
    [Collection(PostgresDockerContainerCollection.Name)]
    public class PostgresAdapterIntegrationTests
    {
        private static readonly ILogger<PostgresAdapter> _logger = NullLogger<PostgresAdapter>.Instance;

        private readonly PostgresDockerContainerFixture _fixture;

        public PostgresAdapterIntegrationTests(PostgresDockerContainerFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task CanPersistNewGame()
        {
            var options = await _fixture.CreateDatabaseAsync(CancellationToken.None);
            var gameId = await CreateGameAsync(options, CancellationToken.None);
            Assert.NotNull(gameId);
        }

        [Fact]
        public async Task CanAddGameEvent()
        {
            var options = await _fixture.CreateDatabaseAsync(CancellationToken.None);
            var gameId = await CreateGameAsync(options, CancellationToken.None);

            var sut = new PostgresAdapter(options, _logger);

            var gameEvent = new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = null,
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = new PlayerId("player 2"),
            };

            await sut.AddEventAsync(gameId, gameEvent, CancellationToken.None);
        }

        [Fact]
        public async Task CanAddGameEventWithCardDrawn()
        {
            var options = await _fixture.CreateDatabaseAsync(CancellationToken.None);
            var gameId = await CreateGameAsync(options, CancellationToken.None);

            var sut = new PostgresAdapter(options, _logger);

            var gameEvent = new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = new Card(DeckNo.One, Suit.Spades, Rank.Five),
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = new PlayerId("player 2"),
            };

            await sut.AddEventAsync(gameId, gameEvent, CancellationToken.None);
        }

        [Fact]
        public async Task CanGetGameList()
        {
            var options = await _fixture.CreateDatabaseAsync(CancellationToken.None);
            var gameId = await CreateGameAsync(options, CancellationToken.None);
            var playerId = new PlayerId("player 1");
            var sut = new PostgresAdapter(options, _logger);
            var gameList = await sut.GetGamesForPlayerAsync(playerId, CancellationToken.None);
            Assert.NotNull(gameList);
            var gameListItem = Assert.Single(gameList.Games);
            Assert.Equal(gameId, gameListItem.GameId);
            Assert.Equal(playerId, gameListItem.NextPlayerId);
            Assert.Equal("player 2", gameListItem.Opponent.ToString());
        }

        [Fact]
        public async Task CanGetGame()
        {
            var options = await _fixture.CreateDatabaseAsync(CancellationToken.None);
            var gameId = await CreateGameAsync(options, CancellationToken.None);
            var sut = new PostgresAdapter(options, _logger);
            var game = await sut.GetGameByIdAsync(gameId, CancellationToken.None);
            Assert.NotNull(game);
        }

        [Fact]
        public async Task CanGetGameWithOneEvent()
        {
            var options = await _fixture.CreateDatabaseAsync(CancellationToken.None);
            var gameId = await CreateGameAsync(options, CancellationToken.None);
            var sut = new PostgresAdapter(options, _logger);
            var gameEvent = new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = null,
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = new PlayerId("player 2"),
            };
            await sut.AddEventAsync(gameId, gameEvent, CancellationToken.None);
            var game = await sut.GetGameByIdAsync(gameId, CancellationToken.None);
            Assert.NotNull(game);
        }

        [Fact]
        public async Task CanGetGameWithOneEventWithCardDrawn()
        {
            var options = await _fixture.CreateDatabaseAsync(CancellationToken.None);
            var gameId = await CreateGameAsync(options, CancellationToken.None);
            var sut = new PostgresAdapter(options, _logger);
            var gameEvent = new GameEvent
            {
                ByPlayerId = new PlayerId("player 1"),
                CardDrawn = new Card(DeckNo.One, Suit.Spades, Rank.Five),
                CardUsed = new Card(DeckNo.Two, Suit.Diamonds, Rank.King),
                Chip = Team.Green,
                Coord = new Coord(4, 2),
                Index = 2,
                NextPlayerId = new PlayerId("player 2"),
            };
            await sut.AddEventAsync(gameId, gameEvent, CancellationToken.None);
            var game = await sut.GetGameByIdAsync(gameId, CancellationToken.None);
            Assert.NotNull(game);
        }

        private async Task<GameId> CreateGameAsync(
            IOptions<PostgresOptions> options,
            CancellationToken cancellationToken)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var sut = new PostgresAdapter(options, _logger);

            var newGame = new NewGame(
                player1: new PlayerId("player 1"),
                player2: new PlayerId("player 2"),
                seed: new Seed(42)
            );

            return await sut.PersistNewGameAsync(newGame, CancellationToken.None);
        }
    }
}
