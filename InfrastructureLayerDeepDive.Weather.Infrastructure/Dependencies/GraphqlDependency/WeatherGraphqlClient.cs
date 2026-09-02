using GraphQL;
using GraphQL.Client.Abstractions;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.GraphqlDependency
{
    public class WeatherGraphqlClient(IGraphQLClient graphQlClient, ILogger<WeatherGraphqlClient> logger) : IWeatherGraphqlClient
    {
        public async Task<IEnumerable<WeatherEntity>?> GetForecastsAsync(CancellationToken cancellationToken)
        {
            var request = new GraphQLRequest
            {
                Query = """
                    query GetForecasts {
                        forecasts {
                            date
                            temperatureC
                            summary
                        }
                    }
                    """
            };

            logger.LogDebug("Sending GraphQL query for weather forecasts");
            var response = await graphQlClient.SendQueryAsync<WeatherForecastsResponse>(request, cancellationToken);

            if (response.Errors is { Length: > 0 })
            {
                logger.LogError("GraphQL errors: {Errors}", string.Join(",", response.Errors.Select(e => e.Message)));
                return null;
            }

            return response.Data?.Forecasts;
        }

        private sealed class WeatherForecastsResponse
        {
            public List<WeatherEntity> Forecasts { get; set; } = [];
        }
    }
}