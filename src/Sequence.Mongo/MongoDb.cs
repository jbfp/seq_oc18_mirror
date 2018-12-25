using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Sequence.Core;
using Sequence.Core.CreateGame;
using Sequence.Core.GetGames;
using Sequence.Core.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Mongo
{
    [Obsolete]
    public sealed class MongoDb : IGameEventStore, IGameProvider, IGameListProvider, IGameStore
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<BsonDocument> _games;

        public MongoDb(IOptions<MongoDbOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _client = new MongoClient(options.Value.ConnectionString);
            _db = _client.GetDatabase("sequence");
            _games = _db.GetCollection<BsonDocument>("games");
        }

        public async Task AddEventAsync(GameId gameId, GameEvent gameEvent, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<BsonDocument>.Filter
                .Eq("game_id", gameId.ToString());

            var update = Builders<BsonDocument>.Update
                .Push("game_events", gameEvent);

            await _games.UpdateOneAsync(filter, update, options: null, cancellationToken);
        }

        public Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            throw new NotImplementedException();
        }

        public Task<GameList> GetGamesForPlayerAsync(PlayerId playerId, CancellationToken cancellationToken)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            throw new NotImplementedException();
        }

        public async Task<GameId> PersistNewGameAsync(NewGame newGame, CancellationToken cancellationToken)
        {
            if (newGame == null)
            {
                throw new ArgumentNullException(nameof(newGame));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var gameId = new GameId(Guid.NewGuid());

            var document = new BsonDocument
            {
                {"game_id", gameId.ToString()},
                {"seed", newGame.Seed.ToInt32()},
                {"player1", newGame.Player1.ToString()},
                {"player2", newGame.Player2.ToString()},
                {"version", 1},
            };

            await _games.InsertOneAsync(document, null, cancellationToken);

            return gameId;
        }
    }
}
