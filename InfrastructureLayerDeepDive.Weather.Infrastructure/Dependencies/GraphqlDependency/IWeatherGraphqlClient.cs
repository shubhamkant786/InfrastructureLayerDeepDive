using InfrastructureLayerDeepDive.Weather.Domain;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.GraphqlDependency
{
    public interface IWeatherGraphqlClient
    {
        Task<IEnumerable<WeatherEntity>?> GetForecastsAsync(CancellationToken cancellationToken);
    }
}