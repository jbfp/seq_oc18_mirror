using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    internal static class CancellationTokenExtensions
    {
        public static Task WaitAsync(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return cancellationToken.IsCancellationRequested ? Task.CompletedTask : tcs.Task;
        }
    }
}
