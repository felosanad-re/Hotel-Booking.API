using Hotel_booking.Core.GenericRepository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Hotel_Booking.Repositories.GenericRepositories
{
    public class RedisReop<T> : IRedisRepo<T>
    {
        #region Services
        protected readonly IDatabase _redisDb;
        protected readonly string _keyPrefix;
        protected readonly ILogger<RedisReop<T>> _logger;

        public RedisReop(IConnectionMultiplexer redis, ILogger<RedisReop<T>> logger)
        {
            _redisDb = redis.GetDatabase();
            _keyPrefix = typeof(T).Name;
            _logger = logger;
        }
        #endregion

        private string GetFullKey(string key) => $"{_keyPrefix}:{key}";

        public async Task<bool> SetKeyAsync(string key, T value, TimeSpan? expire = null)
        {
            try
            {
                var fullKey = GetFullKey(key);

                var json = JsonConvert.SerializeObject(value);

                return await _redisDb.StringSetAsync(fullKey, json, expire.HasValue ? (Expiration)expire : default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<T?> GetKeyAsync(string key)
        {
            try
            {
                var fullKey = GetFullKey(key);
                var value = await _redisDb.StringGetAsync(fullKey);

                if (value.HasValue)
                {
                    return JsonConvert.DeserializeObject<T>(value!);
                }
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return default;
            }
        }

        public async Task<bool> KeyExistAsync(string key)
        {
            var fullKey = GetFullKey(key);
            return await _redisDb.KeyExistsAsync(fullKey);
        }

        public async Task<bool> DeleteKeyAsync(string key)
        {
            try
            {
                var fullKey = GetFullKey(key);
                return await _redisDb.KeyDeleteAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
