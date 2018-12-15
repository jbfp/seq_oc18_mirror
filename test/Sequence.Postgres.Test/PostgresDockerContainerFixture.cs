using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Postgres.Test
{
    [CollectionDefinition(Name)]
    public sealed class PostgresDockerContainerCollection : ICollectionFixture<PostgresDockerContainerFixture>
    {
        public const string Name = "Postgres Docker Container Collection";
    }

    public sealed class PostgresDockerContainerFixture : IDisposable
    {
        public const int Port = 15432;

        private readonly string _containerId;

        public PostgresDockerContainerFixture()
        {
            _containerId = $"docker run --rm -d -p {Port}:5432 postgres".Bash().Trim();
        }

        public async Task<IOptions<PostgresOptions>> CreateDatabaseAsync(CancellationToken cancellationToken)
        {
            // Create test database.
            var connectionString = $"Server=localhost; Port={Port}; Database=postgres; User ID=postgres;";
            var databaseSuffix = Guid.NewGuid();
            var databaseName = $"sequence_{databaseSuffix:N}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await Task.Delay(1000, cancellationToken); // Hopefully resolves timing issue.
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($"CREATE DATABASE {databaseName};");
                connection.ChangeDatabase(databaseName);
                connectionString = connection.ConnectionString;
            }

            // Apply migrations.
            var postgresOptions = new PostgresOptions { ConnectionString = connectionString };
            var options = Options.Create<PostgresOptions>(postgresOptions);
            var logger = NullLogger<PostgresMigrations>.Instance;
            var migrations = new PostgresMigrations(options, logger);
            await migrations.UpgradeDatabaseAsync(cancellationToken);

            return options;
        }

        public void Dispose()
        {
            $"docker stop {_containerId}".Bash();
        }
    }
}
