using Sequence.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.Play
{
    public sealed class PlayHandler
    {
        private readonly IGameProvider _provider;
        private readonly IGameEventStore _store;

        public PlayHandler(IGameProvider provider, IGameEventStore store)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<PlayCardResult> PlayCardAsync(
            GameId gameId,
            PlayerHandle player,
            Card card,
            Coord coord,
            CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Game game = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (game == null)
            {
                throw new GameNotFoundException();
            }

            GameEvent gameEvent = game.PlayCard(player, card, coord);

            await _store.AddEventAsync(gameId, gameEvent, cancellationToken);

            return new PlayCardResult
            {
                CardDrawn = gameEvent.CardDrawn,
                CardUsed = gameEvent.CardUsed,
                Chip = gameEvent.Chip,
                Coord = gameEvent.Coord,
                NextPlayerId = gameEvent.NextPlayerId,
                Version = gameEvent.Index,
            };
        }
    }
}
