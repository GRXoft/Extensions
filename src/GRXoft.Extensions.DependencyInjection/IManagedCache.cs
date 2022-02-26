using System;
using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    public interface IManagedCache
    {
        string Key { get; }

        Type ValueType { get; }

        Task Invalidate(CancellationToken cancellationToken);

        Task Start(CancellationToken cancellationToken);

        Task Stop(CancellationToken cancellationToken);
    }
}
