using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;
using Sequence.Core.Play;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api.Test
{
    internal sealed class InMemoryDatabase : IGameEventStore, IGameProvider, IGameListProvider, IGameStore
    {
        private readonly ConcurrentDictionary<string, NewGame> _games;
        private readonly ConcurrentDictionary<(string, int), GameEvent> _gameEvents;

        public InMemoryDatabase()
        {
            _games = new ConcurrentDictionary<string, NewGame>();
            _gameEvents = new ConcurrentDictionary<(string, int), GameEvent>();
        }

        public Task AddEventAsync(GameId gameId, GameEvent gameEvent, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new System.ArgumentNullException(nameof(gameId));
            }

            if (gameEvent == null)
            {
                throw new System.ArgumentNullException(nameof(gameEvent));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!_gameEvents.TryAdd((gameId.ToString(), gameEvent.Index), gameEvent))
            {
                throw new InvalidOperationException();
            }

            return Task.CompletedTask;
        }

        public Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new System.ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Game game = null;

            if (_games.TryGetValue(gameId.ToString(), out var row))
            {
                var init = new GameInit(row.Player1, row.Player2, row.Seed);

                var gameEvents = _gameEvents
                    .Where(kvp => kvp.Key.Item1 == gameId.ToString())
                    .OrderBy(kvp => kvp.Key.Item2)
                    .Select(kvp => kvp.Value)
                    .ToArray();

                game = new Game(init, gameEvents);
            }

            return Task.FromResult(game);
        }

        public Task<IReadOnlyList<GameId>> GetGamesForPlayerAsync(PlayerId playerId, CancellationToken cancellationToken)
        {
            if (playerId == null)
            {
                throw new System.ArgumentNullException(nameof(playerId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<GameId> gameIds = _games
                .Where(kvp => kvp.Value.Player1.ToString() == playerId.ToString() || kvp.Value.Player2.ToString() == playerId.ToString())
                .Select(kvp => new GameId(kvp.Key))
                .ToList()
                .AsReadOnly();

            return Task.FromResult(gameIds);
        }

        public Task<GameId> PersistNewGameAsync(NewGame newGame, CancellationToken cancellationToken)
        {
            if (newGame == null)
            {
                throw new System.ArgumentNullException(nameof(newGame));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var gameId = new GameId(Guid.NewGuid());

            if (!_games.TryAdd(gameId.ToString(), newGame))
            {
                throw new InvalidOperationException();
            }

            return Task.FromResult(gameId);
        }
    }
}
