using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.PlayCard
{
    public sealed class PlayCardHandler
    {
        private readonly IGameStateProvider _provider;
        private readonly IGameEventStore _store;

        public PlayCardHandler(IGameStateProvider provider, IGameEventStore store)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<GameEvent> PlayCardAsync(
            GameId gameId,
            PlayerHandle player,
            Card card,
            Coord coord,
            CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            GameState state = await GetGameByIdAsync(gameId, cancellationToken);
            GameEvent gameEvent = Game.PlayCard(state, player, card, coord);
            await _store.AddEventAsync(gameId, gameEvent, cancellationToken);
            return gameEvent;
        }

        public async Task<GameEvent> PlayCardAsync(
            GameId gameId,
            PlayerId player,
            Card card,
            Coord coord,
            CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            GameState state = await GetGameByIdAsync(gameId, cancellationToken);
            GameEvent gameEvent = Game.PlayCard(state, player, card, coord);
            await _store.AddEventAsync(gameId, gameEvent, cancellationToken);
            return gameEvent;
        }

        private async Task<GameState> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            GameState state = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (state == null)
            {
                throw new GameNotFoundException();
            }

            return state;
        }
    }
}
