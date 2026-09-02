using System.Net.Http.Json;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.RestDependency
{
    public class WeatherRestClient(ILogger<WeatherRestClient> logger
        , IHttpClientFactory httpClientFactory
        , ITokenAcquisition tokenAcquisition) :
        BaseRestClient<WeatherEntity>(logger, httpClientFactory, tokenAcquisition),
        IWeatherRestClient
    {
        public async Task<IEnumerable<WeatherEntity>?> GetForecastsAsync(CancellationToken cancellationToken)
        {
            var result = await GetAsync("https://localhost:5001", "api/weatherforecast", null, null, cancellationToken);
            return result;
        }

        public async Task<WeatherEntity?> GetForecastByDateAsync(DateOnly date, CancellationToken cancellationToken)
        {
            logger.LogDebug("Requesting weather forecast for {Date}", date);
            var queryStrings = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("date", date.ToString("O"))
            };
            var result = await GetAsync("https://localhost:5001", "api/weatherforecast", queryStrings, null, cancellationToken);
            return result.FirstOrDefault();
        }
    }
}