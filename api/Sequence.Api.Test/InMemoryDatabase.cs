using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;
using Sequence.Core.Play;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api.Test
{
    internal sealed class InMemoryDatabase : IGameEventStore, IGameProvider, IGameListProvider, IGameStore
    {
        private readonly ConcurrentDictionary<GameId, GameInit> _games;
        private readonly ConcurrentDictionary<(GameId, int), GameEvent> _gameEvents;

        public InMemoryDatabase()
        {
            _games = new ConcurrentDictionary<GameId, GameInit>();
            _gameEvents = new ConcurrentDictionary<(GameId, int), GameEvent>();
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

            if (!_gameEvents.TryAdd((gameId, gameEvent.Index), gameEvent))
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

            if (_games.TryGetValue(gameId, out var init))
            {
                var gameEvents = _gameEvents
                    .Where(kvp => kvp.Key.Item1 == gameId)
                    .OrderBy(kvp => kvp.Key.Item2)
                    .Select(kvp => kvp.Value)
                    .ToArray();

                game = new Game(init, gameEvents);
            }

            return Task.FromResult(game);
        }

        public Task<GameList> GetGamesForPlayerAsync(PlayerHandle player, CancellationToken cancellationToken)
        {
            if (player == null)
            {
                throw new System.ArgumentNullException(nameof(player));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var gameListItems = new List<GameListItem>();

            var gameIds = _games
                .Where(kvp => kvp.Value.Players.Any(p => player.Equals(p.Handle)))
                .Select(kvp => kvp.Key)
                .ToList()
                .AsReadOnly();

            foreach (var gameId in gameIds)
            {
                var game = _games[gameId];

                PlayerId nextPlayerId = null;

                var latestGameEventIdx = _gameEvents.Keys
                    .Where(key => key.Item1 == gameId)
                    .OrderByDescending(key => key.Item2)
                    .Select(key => key.Item2)
                    .FirstOrDefault();

                if (_gameEvents.TryGetValue((gameId, latestGameEventIdx), out var gameEvent))
                {
                    nextPlayerId = gameEvent.NextPlayerId;
                }
                else
                {
                    nextPlayerId = _games[gameId].FirstPlayerId;
                }

                var nextPlayerName = game.Players.SingleOrDefault(p => p.Id == nextPlayerId)?.Handle;
                var opponents = game.Players.Select(p => p.Handle).Except(new[] { player }).ToImmutableList();
                var gameListItem = new GameListItem(gameId, nextPlayerName, opponents);
                gameListItems.Add(gameListItem);
            }

            return Task.FromResult(new GameList(gameListItems.ToImmutableList()));
        }

        public Task<GameId> PersistNewGameAsync(NewGame newGame, CancellationToken cancellationToken)
        {
            if (newGame == null)
            {
                throw new System.ArgumentNullException(nameof(newGame));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var gameId = new GameId(Guid.NewGuid());

            var players = newGame.PlayerList
                .Select((player, i) => new Player(new PlayerId(i + 1), player.Handle))
                .ToImmutableList();

            var firstPlayerId = players.Single(p => p.Handle == newGame.PlayerList.FirstPlayer.Handle).Id;
            var init = new GameInit(players, firstPlayerId, newGame.Seed, newGame.BoardType);

            if (!_games.TryAdd(gameId, init))
            {
                throw new InvalidOperationException();
            }

            return Task.FromResult(gameId);
        }
    }
}
