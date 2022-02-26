using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    public interface ICacheLifetime
    {
        void ConfigureNext(bool success);

        /// <summary>
        /// Cuts off Next()
        /// </summary>
        void Interrupt();

        Task Next(CancellationToken cancellationToken);
    }
}
