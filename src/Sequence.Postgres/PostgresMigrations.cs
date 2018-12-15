using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Postgres
{
    public sealed class PostgresMigrations
    {
        private readonly IOptions<PostgresOptions> _options;
        private readonly ILogger _logger;

        public PostgresMigrations(IOptions<PostgresOptions> options, ILogger<PostgresMigrations> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task UpgradeDatabaseAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var migrationsDirectoryPath = Path.Combine(assemblyDirectory, "Migrations");

            var files = Directory
                .EnumerateFiles(migrationsDirectoryPath, "*.sql", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(s => s)
                .AsEnumerable();

            using (var connection = new NpgsqlConnection(_options.Value.ConnectionString))
            {
                _logger.LogInformation("Upgrading database {Database}@{Host}", connection.Database, connection.Host);

                try
                {
                    await connection.OpenAsync(cancellationToken);
                }
                catch (NpgsqlException ex)
                {
                    _logger.LogCritical(ex, "Failed to open connection");
                    throw;
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        _logger.LogDebug("Getting the name of the latest migration applied");

                        string latestMigration = null;

                        {
                            var command = new CommandDefinition(
                                commandText: "SELECT to_regclass('public.migration')::TEXT;",
                                transaction: transaction,
                                cancellationToken: cancellationToken
                            );

                            var result = await connection.QuerySingleOrDefaultAsync<string>(command);

                            if (result != null)
                            {
                                command = new CommandDefinition(
                                    commandText: "SELECT name FROM public.migration ORDER BY id DESC LIMIT 1;",
                                    transaction: transaction,
                                    cancellationToken: cancellationToken
                                );

                                latestMigration = await connection.QuerySingleOrDefaultAsync<string>(command);
                            }
                        }

                        _logger.LogInformation("Upgrading from '{LatestMigration}'", latestMigration);

                        if (latestMigration != null)
                        {
                            files = files
                                .SkipWhile(fileName => !fileName.Equals(latestMigration, StringComparison.OrdinalIgnoreCase))
                                .Skip(1); // Skip 'latest migration'.
                        }

                        foreach (var file in files)
                        {
                            var path = Path.Combine(migrationsDirectoryPath, file + ".sql");

                            string sql;

                            try
                            {
                                sql = File.ReadAllText(path, Encoding.UTF8);
                            }
                            catch (IOException ex)
                            {
                                _logger.LogCritical(ex, "Failed to read migration file for {Migration}", file);
                                throw;
                            }

                            _logger.LogInformation("Applying migration '{Migration}'", file);
                            _logger.LogDebug("{Sql}", sql);

                            var command = new CommandDefinition(
                                commandText: sql,
                                transaction: transaction,
                                cancellationToken: cancellationToken
                            );

                            await connection.ExecuteAsync(command);
                        }

                        _logger.LogInformation("Finished applying all migrations");
                    }
                    catch (NpgsqlException ex)
                    {
                        _logger.LogCritical(ex, "Failed to upgrade database, rolling back");
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }

                    await transaction.CommitAsync(cancellationToken);
                }

                _logger.LogInformation("Database {Database}@{Host} upgraded successfully", connection.Database, connection.Host);
            }
        }
    }
}
