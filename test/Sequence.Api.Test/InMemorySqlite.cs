using Microsoft.Data.Sqlite;
using Sequence.Api.Sqlite;
using System;
using System.Threading;

namespace Sequence.Api.Test
{
    public sealed class InMemorySqlite : IDisposable
    {
        private readonly SqliteConnection _masterConnection;

        public InMemorySqlite()
        {
            var databaseId = Guid.NewGuid().ToString("N");
            var connectionString = $"Data Source=InMemory_{databaseId}; Mode=Memory; Cache=Shared;";
            _masterConnection = new SqliteConnection(connectionString);
            _masterConnection.Open();

            SqliteMigrations
                .UpgradeDatabaseAsync(CreateConnection, CancellationToken.None)
                .Wait();
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_masterConnection.ConnectionString);
        }

        public void Dispose()
        {
            _masterConnection.Dispose();
        }
    }
}
