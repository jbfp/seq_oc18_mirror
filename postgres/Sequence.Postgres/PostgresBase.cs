using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Sequence.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public abstract class PostgresBase
    {
        static PostgresBase()
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
            SqlMapper.AddTypeHandler<PlayerId>(new PlayerIdTypeHandler());
            SqlMapper.AddTypeHandler<Seed>(new SeedTypeHandler());
        }

        private readonly IOptions<PostgresOptions> _options;

        protected PostgresBase(IOptions<PostgresOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected async Task<NpgsqlConnection> CreateAndOpenAsync(CancellationToken cancellationToken)
        {
            var connectionString = _options.Value.ConnectionString;
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
