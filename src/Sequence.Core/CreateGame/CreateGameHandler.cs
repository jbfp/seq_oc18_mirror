using System;
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

        public async Task<GameId> CreateGameAsync(PlayerId player1, PlayerId player2, CancellationToken cancellationToken)
        {
            var seed = await _seedProvider.GenerateSeedAsync(cancellationToken);
            var newGame = new NewGame(player1, player2, player1, seed);
            return await _store.PersistNewGameAsync(newGame, cancellationToken);
        }
    }
}
