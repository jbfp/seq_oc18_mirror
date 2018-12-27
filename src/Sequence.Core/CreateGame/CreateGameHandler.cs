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

        public async Task<GameId> CreateGameAsync(PlayerId player1, PlayerId player2, CancellationToken cancellationToken)
        {
            if (player1 == null)
            {
                throw new ArgumentNullException(nameof(player1));
            }

            if (player2 == null)
            {
                throw new ArgumentNullException(nameof(player2));
            }

            var players = ImmutableList.Create(player1, player2);
            var firstPlayerId = player1;
            var seed = await _seedProvider.GenerateSeedAsync(cancellationToken);
            var newGame = new NewGame(players, firstPlayerId, seed);
            return await _store.PersistNewGameAsync(newGame, cancellationToken);
        }
    }
}
