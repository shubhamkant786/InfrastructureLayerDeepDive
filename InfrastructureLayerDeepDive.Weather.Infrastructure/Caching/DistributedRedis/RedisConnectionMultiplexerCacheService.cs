using Confluent.Kafka;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.DistributedRedis
{
    public class RedisConnectionMultiplexerCacheService(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RedisCacheOptions> options,
        ILogger<RedisConnectionMultiplexerCacheService> logger) : ICacheService
    {
        private readonly RedisCacheOptions _options = options.Value;

        private IDatabase Database => connectionMultiplexer.GetDatabase();

        private string BuildKey(string key) => $"{_options.InstanceName}{key}";

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            var redisValue = await Database.StringGetAsync(BuildKey(key));

            if (!redisValue.HasValue)
            {
                logger.LogDebug("Redis cache miss for key {Key}", key);
                return default;
            }

            logger.LogDebug("Redis cache hit for key {Key}", key);
            return JsonSerializer.Deserialize<T>(redisValue!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(value);
            var expiration = absoluteExpiration ?? _options.DefaultAbsoluteExpiration;

            await Database.StringSetAsync(BuildKey(key), json, expiration);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
            => Database.KeyDeleteAsync(BuildKey(key));

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

        public async Task<IEnumerable<string>> GetKeysAsync(string prefix, CancellationToken cancellationToken)
        {
            var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
            RedisValue pattern = prefix + "*";
            var keys = server.Keys(pattern: $"{_options.InstanceName}{pattern}").Select(k => k.ToString());
            logger.LogTrace($"Total of {keys.Count()} Keys related to prefix {prefix} was found");

            return await Task.FromResult(keys);
        }

        public async Task<long> RemoveKeysAsync(string prefix, CancellationToken cancellationToken)
        {
            var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
            var db = connectionMultiplexer.GetDatabase();

            RedisValue pattern = prefix + "*";
            var keys = server.Keys(pattern: $"{_options.InstanceName}{pattern}");
            var res = db.KeyDelete(keys.ToArray());
            return await Task.FromResult(res);
        }
    }
}