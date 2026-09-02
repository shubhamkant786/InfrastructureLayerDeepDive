using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Cosmos
{
    public class WeatherCosmosRepository : BaseCosmosDbRepository, IWeatherCosmosRepository
    {
        private readonly ILogger<WeatherCosmosRepository> _logger;

        protected override string ContainerName => "WeatherForecasts";

        public WeatherCosmosRepository(CosmosDbContext context, ILogger<WeatherCosmosRepository> logger)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<WeatherEntity> CreateAsync(WeatherEntity entity, CancellationToken cancellationToken)
        {
            var document = WeatherDocument.FromEntity(entity);

            _logger.LogInformation("Creating weather document {Id} in container {ContainerName}", document.Id, ContainerName);

            var response = await _container.CreateItemAsync(
                document,
                new PartitionKey(document.PartitionKey),
                _itemRequestOptions,
                cancellationToken);

            return response.Resource.ToEntity();
        }

        public async Task<WeatherEntity> UpdateAsync(WeatherEntity entity, CancellationToken cancellationToken)
        {
            var document = WeatherDocument.FromEntity(entity);

            _logger.LogInformation("Replacing weather document {Id} in container {ContainerName}", document.Id, ContainerName);

            var response = await _container.ReplaceItemAsync(
                document,
                document.Id,
                new PartitionKey(document.PartitionKey),
                _itemRequestOptions,
                cancellationToken);

            return response.Resource.ToEntity();
        }

        public async Task<WeatherEntity> PatchTemperatureAsync(DateOnly date, int temperatureC, CancellationToken cancellationToken)
        {
            var id = date.ToString("O");
            var partitionKey = new PartitionKey(id);

            var patchOperations = new List<PatchOperation>
            {
                PatchOperation.Replace("/TemperatureC", temperatureC)
            };

            _logger.LogInformation("Patching TemperatureC for weather document {Id} in container {ContainerName}", id, ContainerName);

            var response = await _container.PatchItemAsync<WeatherDocument>(
                id,
                partitionKey,
                patchOperations,
                _patchItemRequestOptions,
                cancellationToken);

            return response.Resource.ToEntity();
        }

        public async Task<WeatherEntity?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var id = date.ToString("O");
            var partitionKey = new PartitionKey(id);

            try
            {
                var response = await _container.ReadItemAsync<WeatherDocument>(id, partitionKey, _itemRequestOptions, cancellationToken);
                return response.Resource.ToEntity();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Weather document {Id} not found in container {ContainerName}", id, ContainerName);
                return null;
            }
        }

        public async Task DeleteAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var id = date.ToString("O");
            var partitionKey = new PartitionKey(id);

            _logger.LogInformation("Deleting weather document {Id} from container {ContainerName}", id, ContainerName);

            await _container.DeleteItemAsync<WeatherDocument>(id, partitionKey, _itemRequestOptions, cancellationToken);
        }
    }
}