using Microsoft.Extensions.Options;

namespace Sequence.Postgres
{
    public sealed class PostgresOptions
    {
        public string ConnectionString { get; set; }
    }
}
