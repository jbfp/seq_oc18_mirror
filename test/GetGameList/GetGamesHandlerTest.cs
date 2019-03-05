using Moq;
using Sequence.GetGameList;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Test.GetGameList
{
    public sealed class GetGameListHandlerTest
    {
        [Fact]
        public void Constructor_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(
                paramName: "provider",
                () => new GetGameListHandler(provider: null)
            );
        }

        private readonly Mock<IGameListProvider> _provider = new Mock<IGameListProvider>();
        private readonly GetGameListHandler _sut;

        public GetGameListHandlerTest()
        {
            _sut = new GetGameListHandler(_provider.Object);
        }

        [Fact]
        public async Task ThrowsWhenPlayerIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                paramName: "player",
                testCode: () => _sut.GetGamesForPlayerAsync(player: null, CancellationToken.None)
            );
        }

        [Fact]
        public async Task ThrowsWhenCanceled()
        {
            var playerId = new PlayerHandle("dummy");
            var cancellationToken = new CancellationToken(canceled: true);

            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: () => _sut.GetGamesForPlayerAsync(playerId, cancellationToken)
            );
        }

        [Theory]
        [InlineData("player 1")]
        [InlineData("42")]
        [InlineData("true")]
        public async Task GetsGamesFromProviderForPlayer(string player)
        {
            // Given:
            var playerId = new PlayerHandle(player);
            var expected = new GameList(ImmutableList<GameListItem>.Empty);

            _provider
                .Setup(p => p.GetGamesForPlayerAsync(playerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // When:
            var actual = await _sut.GetGamesForPlayerAsync(playerId, CancellationToken.None);

            // Then:
            Assert.Equal(expected, actual);
            _provider.VerifyAll();
        }
    }
}
