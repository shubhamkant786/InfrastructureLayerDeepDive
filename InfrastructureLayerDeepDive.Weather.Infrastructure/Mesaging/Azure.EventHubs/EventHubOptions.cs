namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.EventHubs
{
    public class EventHubOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string EventHubName { get; set; } = string.Empty;
        public string ConsumerGroup { get; set; } = "$Default";
        public string BlobStorageConnectionString { get; set; } = string.Empty;
        public string BlobContainerName { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string KeyName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
    }
}