using Moq;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Core.Test.CreateGame
{
    public sealed class CreateGameHandlerTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            var seedProvider = Mock.Of<ISeedProvider>();
            var store = Mock.Of<IGameStore>();

            Assert.Throws<ArgumentNullException>(
                paramName: "seedProvider",
                () => new CreateGameHandler(seedProvider: null, store)
            );

            Assert.Throws<ArgumentNullException>(
                paramName: "store",
                () => new CreateGameHandler(seedProvider, store: null)
            );
        }

        private static readonly PlayerList _twoPlayers = new PlayerList(
            new PlayerId("player 1"),
            new PlayerId("player 2"));

        private readonly Mock<ISeedProvider> _seedProvider = new Mock<ISeedProvider>();
        private readonly Mock<IGameStore> _store = new Mock<IGameStore>();
        private readonly CreateGameHandler _sut;

        public CreateGameHandlerTest()
        {
            _sut = new CreateGameHandler(_seedProvider.Object, _store.Object);
        }

        [Fact]
        public async Task CreateGameAsync_NullArgs()
        {
            var players = ImmutableList<PlayerId>.Empty;

            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "players",
                testCode: () => _sut.CreateGameAsync(null, CancellationToken.None)
            );
        }

        [Theory]
        [InlineData(-60)]
        [InlineData(42)]
        [InlineData(100)]
        public async Task CreateGameAsync_GetsSeedFromProvider(int seed)
        {
            // Given:
            var expected = new Seed(seed);

            _seedProvider
                .Setup(s => s.GenerateSeedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // When:
            await _sut.CreateGameAsync(_twoPlayers, CancellationToken.None);

            // Then:
            _seedProvider.VerifyAll();
        }

        [Theory]
        [InlineData("123")]
        [InlineData(42)]
        [InlineData(true)]
        public async Task CreateGameAsync_ReturnsGameIdFromStore(object gameId)
        {
            var expected = new GameId(gameId);

            _store
                .Setup(s => s.PersistNewGameAsync(It.IsAny<NewGame>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var actual = await _sut.CreateGameAsync(_twoPlayers, CancellationToken.None);

            Assert.Equal(expected, actual);
        }
    }
}
