using Hotel_booking.Core.GenericRepository;
using Hotel_booking.Core.Models;

namespace Hotel_booking.Core.UnitOfWorks
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<T> CreateRepository<T>() where T : ModelBase;
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
