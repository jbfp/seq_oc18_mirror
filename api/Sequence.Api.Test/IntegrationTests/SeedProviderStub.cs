using Sequence.Core;
using Sequence.Core.CreateGame;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api.Test.IntegrationTests
{
    internal sealed class SeedProviderStub : ISeedProvider
    {
        public Seed Value { get; set; } = new Seed(42);

        public Task<Seed> GenerateSeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Value);
        }
    }
}
