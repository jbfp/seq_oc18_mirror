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
            TestPlayer.Get,
            TestPlayer.Get);

        private static readonly BoardType _boardType = BoardType.OneEyedJack;

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
            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "players",
                testCode: () => _sut.CreateGameAsync(null, _boardType, CancellationToken.None)
            );
        }

        [Fact]
        public async Task CreateGameAsync_InvalidBoardType()
        {
            var boardType = (BoardType)1337;

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                paramName: "boardType",
                testCode: () => _sut.CreateGameAsync(_twoPlayers, boardType, CancellationToken.None)
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
            await _sut.CreateGameAsync(_twoPlayers, _boardType, CancellationToken.None);

            // Then:
            _seedProvider.VerifyAll();
        }

        [Theory]
        [InlineData("291bcf7e-45a2-4c5a-b16f-af8f9fa1b2df")]
        [InlineData("6a91eb4b-423a-41aa-8b5f-f5587260a4ed")]
        [InlineData("8dc05e8a-b8e1-4062-ab58-3c7c67436ecb")]
        public async Task CreateGameAsync_ReturnsGameIdFromStore(string gameIdStr)
        {
            var expected = new GameId(Guid.Parse(gameIdStr));

            _store
                .Setup(s => s.PersistNewGameAsync(It.IsAny<NewGame>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var actual = await _sut.CreateGameAsync(_twoPlayers, _boardType, CancellationToken.None);

            Assert.Equal(expected, actual);
        }
    }
}
