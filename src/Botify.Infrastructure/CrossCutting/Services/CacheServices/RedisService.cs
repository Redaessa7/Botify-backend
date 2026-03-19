using Microsoft.Extensions.Caching.Distributed;

namespace Botify.Infrastructure.CrossCutting.Services.CacheServices
{
    public class RedisService
    {
        private readonly IDistributedCache _cache;

        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetStringAsync(string key, string value, TimeSpan ttl)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            await _cache.SetStringAsync(key, value, options);
        }

        public async Task<string?> GetStringAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task DeleteKeyAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }

}
