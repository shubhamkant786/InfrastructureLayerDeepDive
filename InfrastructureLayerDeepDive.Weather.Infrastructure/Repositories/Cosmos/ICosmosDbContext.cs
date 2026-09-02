using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Cosmos
{
    public interface ICosmosDbContext
    {
        public Container GetContainer(string databaseName, string containerName);
        Task<DatabaseResponse?> CreateDatabaseIfNotExistsAsync(string id, ThroughputProperties throughput, CancellationToken cancellationToken);
        Task CreateOrUpdateContainerAsync(Database db, string containerId, string partitionKey, CancellationToken cancellationToken);
    }
}
