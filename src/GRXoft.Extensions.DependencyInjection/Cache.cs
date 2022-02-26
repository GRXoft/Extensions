using System;
using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    internal class Cache<T> : IAsync<T>
    {
        private readonly ICacheLifetime _lifetime;
        private readonly IAsync<T> _source;
        private readonly ICacheStore<T> _store;
        private CacheRunClaim? _currentRunClaim;

        public Cache(IAsync<T> source, ICacheStore<T> store, ICacheLifetime lifetime)
        {
            _source = source;
            _store = store;
            _lifetime = lifetime;
        }

        public async Task<T> Get(CancellationToken cancellationToken)
        {
            return await _source.Get(cancellationToken);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            if (IsRunning())
                return;

            var currentRunClaim = new CacheRunClaim();
            _currentRunClaim = currentRunClaim;
            _ = Task.Run(() => Run(currentRunClaim));

            await Task.CompletedTask;
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            var state = Interlocked.Exchange(ref _currentRunClaim, null);
            state?.Stop();
            state?.Dispose();

            await Task.CompletedTask;
        }

        private bool IsRunning() => _currentRunClaim != null;

        private bool IsRunning(CacheRunClaim claim) => _currentRunClaim == claim;

        private async Task Run(CacheRunClaim claim)
        {
            while (IsRunning(claim))
            {
                try
                {
                    var sourceTask = _source.Get(claim.StopToken);
                    var ignore = !await _store.Update(sourceTask, claim.StopToken);

                    if (!ignore)
                    {
                        switch (sourceTask.Status)
                        {
                            case TaskStatus.RanToCompletion:
                                _lifetime.ConfigureNext(true);
                                break;

                            case TaskStatus.Faulted:
                                _lifetime.ConfigureNext(false);
                                break;
                        }
                    }

                    await _lifetime.Next(claim.BreakToken);
                }
                catch (OperationCanceledException e) when (e.CancellationToken.Equals(stopToken))
                {
                }
            }
        }

        private sealed class CacheRunClaim : IDisposable
        {
            private readonly CancellationTokenSource _stopTokenSource;
            private CancellationTokenSource _invalidateTokenSource;
            private CancellationTokenSource _recycleTokenSource;

            public CacheRunClaim()
            {
                _stopTokenSource = new CancellationTokenSource();
                _invalidateTokenSource = new CancellationTokenSource();
                _recycleTokenSource = CancellationTokenSource.CreateLinkedTokenSource(StopToken, InvalidateToken);
            }

            public CancellationToken BreakToken => _recycleTokenSource.Token;

            public CancellationToken InvalidateToken => _invalidateTokenSource.Token;

            public CancellationToken StopToken => _stopTokenSource.Token;

            public void Dispose()
            {
                _stopTokenSource.Dispose();
                _invalidateTokenSource.Dispose();
                _recycleTokenSource.Dispose();
            }

            public void Invalidate()
            {
                using var invalidateTokenSource = Interlocked.Exchange(ref _invalidateTokenSource, new CancellationTokenSource());
                using var recycleTokenSource = Interlocked.Exchange(ref _recycleTokenSource, CancellationTokenSource.CreateLinkedTokenSource(StopToken, InvalidateToken));

                invalidateTokenSource.Cancel();
            }

            public void Stop()
            {
                _stopTokenSource.Cancel();
            }
        }
    }
}
