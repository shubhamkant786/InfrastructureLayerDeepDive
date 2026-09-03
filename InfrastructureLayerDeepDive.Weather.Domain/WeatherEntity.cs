namespace InfrastructureLayerDeepDive.Weather.Domain
{
    public class WeatherEntity
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public string? Summary { get; set; }

        public IEnumerable<WeatherPoint> WeatherPoints { get; set; } = new List<WeatherPoint>();
    }

    public class WeatherPoint
    {
        public int MaxTemperatureC { get; set; }
        public int MinTemperatureC { get; set; }
    }
}
