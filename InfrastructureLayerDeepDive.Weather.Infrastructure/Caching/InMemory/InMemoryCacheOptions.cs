namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Caching.InMemory
{
    public class InMemoryCacheOptions
    {
        public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);
        public long? SizeLimit { get; set; }
    }
}