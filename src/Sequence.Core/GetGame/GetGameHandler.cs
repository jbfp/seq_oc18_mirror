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

        public async Task<GameView> GetGameViewForPlayerAsync(GameId gameId, PlayerId playerId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var game = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (game == null)
            {
                throw new GameNotFoundException();
            }

            return game.GetViewForPlayer(playerId);
        }
    }
}
