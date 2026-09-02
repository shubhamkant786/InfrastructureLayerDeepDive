namespace InfrastructureLayerDeepDive.Weather.Domain
{
    public class WeatherEntity
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public string? Summary { get; set; }
    }
}
