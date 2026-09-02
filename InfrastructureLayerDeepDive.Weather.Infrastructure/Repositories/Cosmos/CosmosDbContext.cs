using Confluent.Kafka;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;


namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Cosmos
{
    public class CosmosDbContext(IOptions<CosmosDbOptions> dbOptions
        , ILogger<CosmosDbContext> logger) : ICosmosDbContext, IDisposable
    {
        private readonly CosmosDbOptions _options = dbOptions.Value;

        private readonly CosmosClient _cosmosClient = new(
            dbOptions.Value.EndpointUri,
            dbOptions.Value.PrimaryKey,
            new CosmosClientOptions
            {
                ApplicationName = "WeatherApp",
                //Always use Direct Mode for performance
                ConnectionMode = ConnectionMode.Direct,// Recommended by Microsoft for lower latency
                ConsistencyLevel = ConsistencyLevel.Session,
                AllowBulkExecution = true,                             

                //TCP Connection Management (Prevent Port Exhaustion & Improve Stability)
                EnableTcpConnectionEndpointRediscovery = true, // Recommended to auto-recover from bad connections
                IdleTcpConnectionTimeout = TimeSpan.FromMinutes(20), // Microsoft recommended range: 20 min - 24 hours

                //Connection Limits - Balance load and prevent excessive connections
                MaxTcpConnectionsPerEndpoint = 100, // Microsoft suggests a high limit for better throughput
                MaxRequestsPerTcpConnection = 30, // Default: 30, Keep as-is for balanced performance

                //Retry Policy (Handle Rate-Limiting and Service Unavailability)
                MaxRetryAttemptsOnRateLimitedRequests = 90, // Default: 9 (Keep the same as Prod)
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(120), // Default: 30s (Keep the same as Prod)

                //Request Handling (Prevent 503 Errors)
                RequestTimeout = TimeSpan.FromSeconds(130), // Default: 65s, Increase to prevent request failures

                EnableContentResponseOnWrite = false,

                //Ensure Consistent Serialization
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase // Recommended for consistent JSON formatting
                }
            });

        public Container GetContainer(string databaseName, string containerName)
        {
            return _cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<DatabaseResponse?> CreateDatabaseIfNotExistsAsync(string id, ThroughputProperties throughput, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating database {DatabaseId} if it does not exist", id);
            ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

            return await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                            id: id,
                            throughputProperties: throughput,
                            cancellationToken: cancellationToken);

        }

        public async Task CreateOrUpdateContainerAsync(Database db, string containerId, string partitionKey, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating or updating container {ContainerId} with partition key {PartitionKey}", containerId, partitionKey);
            ArgumentNullException.ThrowIfNull(db, nameof(db));
            ArgumentException.ThrowIfNullOrWhiteSpace(containerId, nameof(containerId));
            ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey, nameof(partitionKey));


            var containerSettings = _options.Containers.FirstOrDefault(c => c.Id == containerId);
            if (containerSettings == null)
            {
                logger.LogWarning("No settings found for container {ContainerId}. Default settings will apply", containerId);

                containerSettings = CosmosContainerSettings.Default;
            }

            // Try to create the container
            ContainerResponse response;
            try
            {
                logger.LogInformation("Creating container {ContainerId} if not exists", containerId);

                response = await db.CreateContainerIfNotExistsAsync(
                    new ContainerProperties(containerId, partitionKey)
                    {
                        DefaultTimeToLive = containerSettings.Ttl
                    },
                    throughputProperties: ThroughputProperties.CreateAutoscaleThroughput(containerSettings.AutoscaleMaxRu),
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                if (_options.FailOnProvisioningError)
                {
                    logger.LogCritical(ex, "Failed to create container {ContainerId}. App startup failed", containerId);

                    throw;
                }

                logger.LogWarning(ex, "Failed to create container {ContainerId}. Continuing app startup", containerId);

                return;
            }

            // Container was just created, RUs and TTL are set — nothing to update
            if (response.StatusCode == HttpStatusCode.Created)
            {
                logger.LogInformation("Container {ContainerId} was created successfully", containerId);

                return;
            }

            logger.LogInformation("Container {ContainerId} already exists, checking for updates", containerId);

            // Update the RUs only if changed
            try
            {
                var throughput = await response.Container.ReadThroughputAsync(cancellationToken: cancellationToken);

                if (throughput == null)
                {
                    logger.LogWarning("Container {ContainerId} is using shared database throughput. Skipping RUs Updating", containerId);
                }
                else if (throughput != containerSettings.AutoscaleMaxRu)
                {
                    logger.LogInformation("Updating throughput for container {ContainerId} from {Current} to {New} RUs",
                        containerId, throughput, containerSettings.AutoscaleMaxRu);

                    /*
                     * Replacing Throughput hapenning in background, and it take up to 4H to complete
                     * During this time if a second deployment happen, this will raise a 429 exception (if MaxRUs has changed also)
                     * CosmosDb Reflect the value immediatly so the second deployment happen with the same value it will not be an issue
                     * But if we change the RUs again, the second deployment will not apply the new RUs
                     */
                    await response.Container.ReplaceThroughputAsync(
                        ThroughputProperties.CreateAutoscaleThroughput(containerSettings.AutoscaleMaxRu),
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    logger.LogInformation("Throughput for container {ContainerId} is already set to {MaxRu} RU/s. Skipping update",
                        containerId, containerSettings.AutoscaleMaxRu);
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning("Throughput update for container {ContainerId} is still in progress. " +
                                   "The previous change has not been applied yet. Skipping", containerId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to update throughput for container {ContainerId}. Container is still functional", containerId);
            }

            // Update the TTL (only if changed)
            try
            {
                var props = await response.Container.ReadContainerAsync(cancellationToken: cancellationToken);

                if (props.Resource.DefaultTimeToLive != containerSettings.Ttl)
                {
                    logger.LogInformation("Updating TTL for container {ContainerId} from {Current} to {New}",
                        containerId, props.Resource.DefaultTimeToLive, containerSettings.Ttl);

                    props.Resource.DefaultTimeToLive = containerSettings.Ttl;
                    await response.Container.ReplaceContainerAsync(props.Resource, cancellationToken: cancellationToken);
                }
                else
                {
                    logger.LogInformation("TTL for container {ContainerId} is already set to {Ttl}. Skipping update",
                        containerId, containerSettings.Ttl);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to update TTL for container {ContainerId}. Container is still functional", containerId);
            }
        }       

        public void Dispose()
        {
            _cosmosClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class CosmosContainerSettings
    {
        public string Id { get; set; } = default!;
        public int AutoscaleMaxRu { get; set; }
        public int Ttl { get; set; }

        public static CosmosContainerSettings Default => new CosmosContainerSettings
        {
            AutoscaleMaxRu = 4000,
            Ttl = 15778463 // 6 Months
        };

    }
}
