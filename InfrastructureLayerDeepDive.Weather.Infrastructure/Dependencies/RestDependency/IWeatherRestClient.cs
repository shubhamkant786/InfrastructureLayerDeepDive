using InfrastructureLayerDeepDive.Weather.Domain;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.RestDependency
{
    public interface IWeatherRestClient
    {
        Task<IEnumerable<WeatherEntity>?> GetForecastsAsync(CancellationToken cancellationToken);
        Task<WeatherEntity?> GetForecastByDateAsync(DateOnly date, CancellationToken cancellationToken);
    }
}