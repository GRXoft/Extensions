using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    public interface IAsync<T>
    {
        Task<T> Get(CancellationToken cancellationToken);
    }
}
