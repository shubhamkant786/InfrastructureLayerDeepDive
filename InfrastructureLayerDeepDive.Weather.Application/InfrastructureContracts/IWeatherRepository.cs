using InfrastructureLayerDeepDive.Weather.Domain;

namespace InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts
{
    public interface IWeatherRepository
    {
        Task<IEnumerable<WeatherEntity>> GetAllAsync(CancellationToken cancellationToken);
        Task<WeatherEntity?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken);
        Task AddAsync(WeatherEntity entity, CancellationToken cancellationToken);
        Task UpdateAsync(WeatherEntity entity, CancellationToken cancellationToken);
        Task DeleteAsync(DateOnly date, CancellationToken cancellationToken);
    }
}