using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.GRPCDependency
{
    //public class WeatherGrpcClient(WeatherService.WeatherServiceClient grpcClient, ILogger<WeatherGrpcClient> logger) : IWeatherGrpcClient
    //{
    //    public async Task<IEnumerable<WeatherEntity>> GetForecastsAsync(CancellationToken cancellationToken)
    //    {
    //        logger.LogDebug("Calling gRPC GetForecasts");
    //        var response = await grpcClient.GetForecastsAsync(new GetForecastsRequest(), cancellationToken: cancellationToken);

    //        return response.Forecasts.Select(f => new WeatherEntity
    //        {
    //            Date = DateOnly.Parse(f.Date),
    //            TemperatureC = f.TemperatureC,
    //            Summary = f.Summary
    //        });
    //    }
    //}
}