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

            SqlMapper.AddTypeHandler(new GameIdTypeHandler());
            SqlMapper.AddTypeHandler(new PlayerHandleTypeHandler());
            SqlMapper.AddTypeHandler(new PlayerIdTypeHandler());
            SqlMapper.AddTypeHandler(new SeedTypeHandler());
        }

        public NpgsqlConnectionFactory(IOptions<PostgresOptions> options)
        {
            ConnectionString = options.Value.ConnectionString
                ?? throw new ArgumentNullException(nameof(options.Value.ConnectionString));
        }

        public string ConnectionString { get; }

        public async Task<NpgsqlConnection> CreateAndOpenAsync(CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
