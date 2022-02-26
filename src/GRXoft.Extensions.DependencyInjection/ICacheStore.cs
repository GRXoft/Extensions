using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    public interface ICacheStore<T>
    {
        Task<T> Read(CancellationToken cancellationToken);

        Task<bool> Update(Task<T> task, CancellationToken cancellationToken);
    }
}
