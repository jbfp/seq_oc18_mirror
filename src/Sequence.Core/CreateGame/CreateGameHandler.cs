using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.CreateGame
{
    public sealed class CreateGameHandler
    {
        private readonly IGameStore _store;

        public CreateGameHandler(IGameStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<GameId> CreateGameAsync(PlayerId player1, PlayerId player2, CancellationToken cancellationToken)
        {
            return await _store.PersistNewGameAsync(
                new NewGame(player1, player2),
                cancellationToken
            );
        }
    }
}
