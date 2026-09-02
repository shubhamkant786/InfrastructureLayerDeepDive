namespace InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration, CancellationToken cancellationToken);
        Task RemoveAsync(string key, CancellationToken cancellationToken);
        Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? absoluteExpiration, CancellationToken cancellationToken);
    }
}