using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    public static class TaskExtensions
    {
        public static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tsc = new TaskCompletionSource<T>();
            using (cancellationToken.Register(() => tsc.TrySetCanceled(cancellationToken)))
            {
                return await Task.WhenAny(task, tsc.Task).Unwrap().ConfigureAwait(false);
            }
        }
    }
}
