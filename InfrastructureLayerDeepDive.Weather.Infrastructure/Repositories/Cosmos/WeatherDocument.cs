using System.Text.Json.Serialization;
using InfrastructureLayerDeepDive.Weather.Domain;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Cosmos
{
    public class WeatherDocument
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        public string PartitionKey { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public string? Summary { get; set; }

        public static WeatherDocument FromEntity(WeatherEntity entity)
        {
            var dateKey = entity.Date.ToString("O");
            return new WeatherDocument
            {
                Id = dateKey,
                PartitionKey = dateKey,
                Date = entity.Date,
                TemperatureC = entity.TemperatureC,
                Summary = entity.Summary
            };
        }

        public WeatherEntity ToEntity()
        {
            return new WeatherEntity
            {
                Date = Date,
                TemperatureC = TemperatureC,
                Summary = Summary
            };
        }
    }
}