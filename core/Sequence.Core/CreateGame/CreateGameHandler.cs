using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.CreateGame
{
    public sealed class CreateGameHandler
    {
        private readonly ISeedProvider _seedProvider;
        private readonly IGameStore _store;

        public CreateGameHandler(ISeedProvider seedProvider, IGameStore store)
        {
            _seedProvider = seedProvider ?? throw new ArgumentNullException(nameof(seedProvider));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<GameId> CreateGameAsync(
            PlayerList players,
            BoardType boardType,
            int numSequencesToWin,
            CancellationToken cancellationToken)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            var seed = await _seedProvider.GenerateSeedAsync(cancellationToken);
            var newGame = new NewGame(players, seed, boardType, numSequencesToWin);
            return await _store.PersistNewGameAsync(newGame, cancellationToken);
        }
    }
}
