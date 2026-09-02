using InfrastructureLayerDeepDive.Weather.Domain;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.GRPCDependency
{
    public interface IWeatherGrpcClient
    {
        Task<IEnumerable<WeatherEntity>> GetForecastsAsync(CancellationToken cancellationToken);
    }
}