using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    public interface ICacheStore<T>
    {
        Task<T> Read(CancellationToken cancellationToken);

        Task Update(Task<T> task, CancellationToken cancellationToken);
    }
}
