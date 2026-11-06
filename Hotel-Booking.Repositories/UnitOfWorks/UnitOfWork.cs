using Hotel_booking.Core.GenericRepository;
using Hotel_booking.Core.Models;
using Hotel_booking.Core.UnitOfWorks;
using Hotel_Booking.Repositories.Data;
using Hotel_Booking.Repositories.GenericRepositories;

namespace Hotel_Booking.Repositories.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HotelDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed;

        public UnitOfWork(HotelDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IGenericRepository<T> CreateRepository<T>() where T : ModelBase
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var entityType = typeof(T);
            if (_repositories.TryGetValue(entityType, out var repository))
                return (IGenericRepository<T>)repository;

            var newRepository = new GenericRepo<T>(_dbContext);
            _repositories[entityType] = newRepository;
            return newRepository;
        }

        public Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            // To Stop Using UnitOfWork After DisposeAsync
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;
            await _dbContext.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
