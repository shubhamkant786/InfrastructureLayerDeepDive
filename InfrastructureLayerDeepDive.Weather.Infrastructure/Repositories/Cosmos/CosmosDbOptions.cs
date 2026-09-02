using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Cosmos
{
    public class CosmosDbOptions
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public bool FailOnProvisioningError { get; set; }
        public List<CosmosContainerSettings> Containers { get; set; } = new();
    }
}
