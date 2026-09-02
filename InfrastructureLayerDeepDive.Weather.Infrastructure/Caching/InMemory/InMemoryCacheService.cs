using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.InMemory
{
    public class InMemoryCacheService(
        IMemoryCache memoryCache,
        IOptions<InMemoryCacheOptions> options,
        ILogger<InMemoryCacheService> logger) : ICacheService
    {
        private readonly InMemoryCacheOptions _options = options.Value;

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            memoryCache.TryGetValue(key, out T? value);
            logger.LogDebug("InMemory cache {Result} for key {Key}", value is null ? "miss" : "hit", key);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken)
        {
            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _options.DefaultAbsoluteExpiration,
                Size = 1
            };

            memoryCache.Set(key, value, entryOptions);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? absoluteExpiration, CancellationToken cancellationToken)
        {
            if (memoryCache.TryGetValue(key, out T? cached) && cached is not null)
            {
                return cached;
            }

            var value = await factory(cancellationToken);
            await SetAsync(key, value, absoluteExpiration, cancellationToken);
            return value;
        }
    }
}