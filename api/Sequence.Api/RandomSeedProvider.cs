using Microsoft.Extensions.Logging;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class RandomSeedProvider : ISeedProvider
    {
        private static readonly Random _random = new Random();

        private readonly ILogger _logger;

        public RandomSeedProvider(ILogger<RandomSeedProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Seed> GenerateSeedAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rng = _random.Next();
            var seed = new Seed(rng);
            return Task.FromResult(seed);
        }
    }
}
