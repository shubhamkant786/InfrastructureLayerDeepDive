using System.Text.Json;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.DistributedRedis
{
    public class RedisCacheService(
        IDistributedCache distributedCache,
        IOptions<RedisCacheOptions> options,
        ILogger<RedisCacheService> logger) : ICacheService
    {
        private readonly RedisCacheOptions _options = options.Value;

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            var bytes = await distributedCache.GetAsync(key, cancellationToken);
            if (bytes is null || bytes.Length == 0)
            {
                logger.LogDebug("Redis cache miss for key {Key}", key);
                return default;
            }

            logger.LogDebug("Redis cache hit for key {Key}", key);
            return JsonSerializer.Deserialize<T>(bytes);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _options.DefaultAbsoluteExpiration
            };

            await distributedCache.SetAsync(key, bytes, entryOptions, cancellationToken);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
            => distributedCache.RemoveAsync(key, cancellationToken);

        public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? absoluteExpiration, CancellationToken cancellationToken)
        {
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var value = await factory(cancellationToken);
            await SetAsync(key, value, absoluteExpiration, cancellationToken);
            return value;
        }
    }
}


