using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.CreateGame
{
    public sealed class CreateGameHandler
    {
        private readonly IRandomFactory _randomFactory;
        private readonly IGameStore _gameStore;

        public CreateGameHandler(IRandomFactory randomFactory, IGameStore gameStore)
        {
            _randomFactory = randomFactory ?? throw new ArgumentNullException(nameof(randomFactory));
            _gameStore = gameStore ?? throw new ArgumentNullException(nameof(gameStore));
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

            var random = _randomFactory.Create();
            var seed = new Seed(random.Next());
            var newGame = new NewGame(players, seed, boardType, numSequencesToWin);
            return await _gameStore.PersistNewGameAsync(newGame, cancellationToken);
        }
    }
}
