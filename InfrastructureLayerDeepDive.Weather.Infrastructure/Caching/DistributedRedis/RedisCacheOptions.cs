namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.DistributedRedis
{
    public class RedisCacheOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = "WeatherApp:";
        public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);
        public bool Enabled { get; set; }
        public string? Host { get; set; }
        public string? Password { get; set; }
        public string? Ssl { get; set; }
        public string? AbortConnect { get; set; }
        public int? ConnectTimeout { get; set; }
    }
}