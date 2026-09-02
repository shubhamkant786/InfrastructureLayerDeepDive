using System.Text.Json;
using global::Azure.Storage.Blobs;
using global::Azure.Storage.Blobs.Models;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.BlobStorage
{
    public class BlobStorageWeatherProducer : IMessageProducer<WeatherEntity>
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageWeatherProducer> _logger;

        public BlobStorageWeatherProducer(IOptions<BlobStorageOptions> options, ILogger<BlobStorageWeatherProducer> logger)
        {
            _logger = logger;
            var serviceClient = new BlobServiceClient(options.Value.ConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(options.Value.ContainerName);
        }

        public async Task PublishAsync(WeatherEntity message, CancellationToken cancellationToken)
        {
            await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobName = $"{message.Date:O}.json";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var payload = JsonSerializer.SerializeToUtf8Bytes(message);
            using var stream = new MemoryStream(payload);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" }
            };

            await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);

            _logger.LogInformation("Uploaded weather blob {BlobName} to container {ContainerName}", blobName, _containerClient.Name);
        }
    }
}