using Microsoft.Azure.Cosmos;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Cosmos
{
    public abstract class BaseCosmosDbRepository
    {
        protected Container _container;
        protected CosmosClientOptions _cosmosClientOptions;
        protected QueryRequestOptions _queryRequestOptions;
        protected PatchItemRequestOptions _patchItemRequestOptions;
        protected TransactionalBatchRequestOptions _transactionalBatchRequestOptions;
        protected ItemRequestOptions _itemRequestOptions;
        protected abstract string ContainerName { get; }

        protected BaseCosmosDbRepository(CosmosDbContext context)
        {
            _container = context.GetContainer("WeatherDatabase", ContainerName);
            _cosmosClientOptions = new CosmosClientOptions
            {
                ApplicationName = "WeatherApp",
                ConnectionMode = ConnectionMode.Direct,
                ConsistencyLevel = ConsistencyLevel.Session,
                AllowBulkExecution = true
            };
            _queryRequestOptions = new QueryRequestOptions
            {
                MaxItemCount = -1,
                MaxBufferedItemCount = -1,
                MaxConcurrency = -1,
                #if DEBUG
                PopulateIndexMetrics = true
                #endif
            };
            _patchItemRequestOptions = new PatchItemRequestOptions
            {
                EnableContentResponseOnWrite = false
            };
            _transactionalBatchRequestOptions = new TransactionalBatchRequestOptions
            {                
            };
            _itemRequestOptions = new ItemRequestOptions
            {
                EnableContentResponseOnWrite = false
            };
        }
    }
}
