namespace Hotel_booking.Core.GenericRepository
{
    public interface IRedisRepo<T>
    {
        // Set Key
        Task<bool> SetKeyAsync(string  key, T value, TimeSpan? expire = null);

        // Get Key
        Task<T?> GetKeyAsync(string key);

        // Delete Key
        Task<bool> DeleteKeyAsync(string key);

        // Key Exist
        Task<bool> KeyExistAsync(string key);
    }
}
