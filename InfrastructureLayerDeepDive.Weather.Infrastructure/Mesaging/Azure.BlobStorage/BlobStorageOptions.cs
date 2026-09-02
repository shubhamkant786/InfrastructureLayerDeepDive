namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.BlobStorage
{
    public class BlobStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}