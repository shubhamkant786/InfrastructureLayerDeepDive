using System.Text.Json;
using global::Azure.Storage.Blobs;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.BlobStorage
{
    public class BlobStorageWeatherConsumer : IMessageConsumer<WeatherEntity>
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageWeatherConsumer> _logger;
        private CancellationTokenSource? _internalCts;
        private Task? _pollingLoop;

        public BlobStorageWeatherConsumer(IOptions<BlobStorageOptions> options, ILogger<BlobStorageWeatherConsumer> logger)
        {
            _logger = logger;
            var serviceClient = new BlobServiceClient(options.Value.ConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(options.Value.ContainerName);
        }

        public Task StartAsync(Func<WeatherEntity, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken)
        {
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _pollingLoop = Task.Run(async () =>
            {
                await _containerClient.CreateIfNotExistsAsync(cancellationToken: _internalCts.Token);

                await foreach (var blobItem in _containerClient.GetBlobsAsync(cancellationToken: _internalCts.Token))
                {
                    if (_internalCts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                        var download = await blobClient.DownloadContentAsync(_internalCts.Token);

                        var entity = JsonSerializer.Deserialize<WeatherEntity>(download.Value.Content.ToArray());
                        if (entity is not null)
                        {
                            await onMessageReceived(entity, _internalCts.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing blob {BlobName} from container {ContainerName}", blobItem.Name, _containerClient.Name);
                    }
                }
            }, _internalCts.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _internalCts?.Cancel();
            if (_pollingLoop is not null)
            {
                await _pollingLoop;
            }
        }
    }
}