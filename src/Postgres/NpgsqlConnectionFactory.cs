using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class NpgsqlConnectionFactory
    {
        static NpgsqlConnectionFactory()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<DeckNo>("deckno");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<PlayerType>("player_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Rank>("rank");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Suit>("suit");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Team>("chip");

            NpgsqlConnection.GlobalTypeMapper.MapComposite<CardComposite>("card");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<CoordComposite>("coord");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<SequenceComposite>("sequence");

            SqlMapper.AddTypeHandler<GameId>(new GameIdTypeHandler());
            SqlMapper.AddTypeHandler<PlayerHandle>(new PlayerHandleTypeHandler());
            SqlMapper.AddTypeHandler<PlayerId>(new PlayerIdTypeHandler());
            SqlMapper.AddTypeHandler<Seed>(new SeedTypeHandler());
        }

        private readonly IOptions<PostgresOptions> _options;

        public NpgsqlConnectionFactory(IOptions<PostgresOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string ConnectionString => _options.Value.ConnectionString;

        public async Task<NpgsqlConnection> CreateAndOpenAsync(CancellationToken cancellationToken)
        {
            var connectionString = _options.Value.ConnectionString;
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
