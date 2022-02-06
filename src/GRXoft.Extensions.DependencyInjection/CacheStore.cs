using System;
using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    internal class CacheStore<T> : ICacheStore<T>
    {
        private Task<T>? _current;
        private Task<T>? _next;

        public async Task<T> Read(CancellationToken cancellationToken)
        {
            var task = _current ?? _next ?? throw new InvalidOperationException();

            return await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task Update(Task<T> source, CancellationToken cancellationToken)
        {
            _next = source ?? throw new ArgumentNullException(nameof(source));

            try
            {
                await source.WaitAsync(cancellationToken).ConfigureAwait(false);
                OnUpdateSucceeded(source);
            }
            catch
            {
                OnUpdateFailed(source);
                throw;
            }
        }

        private bool ClearNext(Task<T> source)
        {
            // Set '_next' to 'null' if it equals 'source'.
            var originalNext = Interlocked.CompareExchange(ref _next, null, source);

            // If 'source' does not equal 'originalNext' then '_next' was not cleared.
            return source.Equals(originalNext);
        }

        private void OnUpdateFailed(Task<T> source)
        {
            ClearNext(source);
        }

        private void OnUpdateSucceeded(Task<T> source)
        {
            if (ClearNext(source))
                _current = source;
        }
    }
}
