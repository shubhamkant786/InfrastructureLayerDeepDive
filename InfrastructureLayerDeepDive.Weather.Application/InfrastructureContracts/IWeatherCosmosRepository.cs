using InfrastructureLayerDeepDive.Weather.Domain;

namespace InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts
{
    public interface IWeatherCosmosRepository
    {
        Task<WeatherEntity> CreateAsync(WeatherEntity entity, CancellationToken cancellationToken);
        Task<WeatherEntity> UpdateAsync(WeatherEntity entity, CancellationToken cancellationToken);
        Task<WeatherEntity> PatchTemperatureAsync(DateOnly date, int temperatureC, CancellationToken cancellationToken);
        Task<WeatherEntity?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken);
        Task DeleteAsync(DateOnly date, CancellationToken cancellationToken);
    }
}