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
    public sealed class MongoDb : IGameEventStore, IGameProvider, IGameListProvider, IGameStore
    {
        private static readonly Random _random = new Random();

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

        public async Task<Game> GetGameByIdAsync(GameId gameId, CancellationToken cancellationToken)
        {
            if (gameId == null)
            {
                throw new ArgumentNullException(nameof(gameId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<BsonDocument>.Filter
                .Eq("game_id", gameId.ToString());

            var projection = Builders<BsonDocument>.Projection
                .Exclude("_id")
                .Include("player1")
                .Include("player2")
                .Include("seed");

            var document = await _games
                .Find(filter)
                .Project(projection)
                .SingleOrDefaultAsync(cancellationToken);

            if (document == null)
            {
                return null;
            }

            var init = new GameInit(
                player1: new PlayerId(document["player1"].AsString),
                player2: new PlayerId(document["player2"].AsString),
                seed: new Seed(document["seed"].AsInt32)
            );

            return new Game(init);
        }

        public async Task<IReadOnlyList<GameId>> GetGamesForPlayerAsync(PlayerId playerId, CancellationToken cancellationToken)
        {
            if (playerId == null)
            {
                throw new ArgumentNullException(nameof(playerId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Eq("player1", playerId.ToString()),
                Builders<BsonDocument>.Filter.Eq("player2", playerId.ToString())
            );

            var projection = Builders<BsonDocument>.Projection
                .Exclude("_id")
                .Include("game_id");

            var sort = Builders<BsonDocument>.Sort
                .Descending("_id");

            var gameIds = await _games
                .Find(filter)
                .Project(projection)
                .Sort(sort)
                .ToListAsync(cancellationToken);

            return gameIds
                .Select(x => new GameId(x["game_id"].AsGuid))
                .ToList()
                .AsReadOnly();
        }

        public async Task<GameId> PersistNewGameAsync(NewGame newGame, CancellationToken cancellationToken)
        {
            if (newGame == null)
            {
                throw new ArgumentNullException(nameof(newGame));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var gameId = new GameId(Guid.NewGuid());
            var seed = _random.Next();

            var document = new BsonDocument
            {
                {"game_id", gameId.ToString()},
                {"seed", seed},
                {"player1", newGame.Player1.ToString()},
                {"player2", newGame.Player2.ToString()},
                {"version", 1},
            };

            await _games.InsertOneAsync(document, null, cancellationToken);

            return gameId;
        }
    }
}
