using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    internal sealed class RandomSeedProvider : ISeedProvider
    {
        private static readonly Random _random = new Random();

        public Task<Seed> GenerateSeedAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rng = _random.Next();
            var seed = new Seed(rng);
            return Task.FromResult(seed);
        }
    }
}
