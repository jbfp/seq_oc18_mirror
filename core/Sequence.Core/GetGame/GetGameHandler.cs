using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.GetGame
{
    public sealed class GetGameHandler
    {
        private readonly IGameProvider _provider;

        public GetGameHandler(IGameProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task<GameView> GetGameViewForPlayerAsync(GameId gameId, PlayerHandle player, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var game = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (game == null)
            {
                throw new GameNotFoundException();
            }

            return game.GetViewForPlayer(player);
        }
    }
}
