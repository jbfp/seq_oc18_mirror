using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.GetGameView
{
    public sealed class GetGameViewHandler
    {
        private readonly IGameStateProvider _provider;

        public GetGameViewHandler(IGameStateProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task<GameView> GetGameViewForPlayerAsync(
            GameId gameId,
            PlayerHandle player,
            CancellationToken cancellationToken)
        {
            var game = await GetGameStateByIdAsync(gameId, cancellationToken);
            return GameView.FromGameState(game, player);
        }

        public async Task<GameView> GetGameViewForPlayerAsync(
            GameId gameId,
            PlayerId player,
            CancellationToken cancellationToken)
        {
            var game = await GetGameStateByIdAsync(gameId, cancellationToken);
            return GameView.FromGameState(game, player);
        }

        private async Task<GameState> GetGameStateByIdAsync(
            GameId gameId,
            CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var game = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (game == null)
            {
                throw new GameNotFoundException();
            }

            return game;
        }
    }
}
