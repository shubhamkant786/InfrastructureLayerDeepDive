using System.Text.Json;
using Azure.Messaging.EventHubs.Processor;
using global::Azure.Messaging.EventHubs;
using global::Azure.Messaging.EventHubs.Consumer;
using global::Azure.Messaging.EventHubs.Primitives;
using global::Azure.Storage.Blobs;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Application.Models;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.EventHubs
{
    public class EventHubWeatherConsumer : IMessageConsumer<WeatherModel>
    {
        private readonly EventHubOptions _options;
        private readonly ILogger<EventHubWeatherConsumer> _logger;
        private EventProcessorClient? _processorClient;

        public EventHubWeatherConsumer(IOptions<EventHubOptions> options
            , ILogger<EventHubWeatherConsumer> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public Func<WeatherEntity, CancellationToken, Task> ProcessMessageAsync { get; set; }

        public Func<Exception, CancellationToken, Task> ProcessErrorAsync { get; set; }

        public async Task StartAsync(Func<WeatherModel, CancellationToken, Task> onMessageReceived
            , CancellationToken cancellationToken)
        {
            var storageClient = new BlobContainerClient(_options.BlobStorageConnectionString, _options.BlobContainerName);

            _processorClient = new EventProcessorClient(
                storageClient,
                _options.ConsumerGroup,
                _options.ConnectionString,
                _options.EventHubName,
                new EventProcessorClientOptions
                {
                    Identifier = Guid.NewGuid().ToString(),
                    MaximumWaitTime = TimeSpan.FromSeconds(60),
                    RetryOptions = new EventHubsRetryOptions
                    {
                        Mode = EventHubsRetryMode.Exponential,
                        Delay = TimeSpan.FromSeconds(0.5),
                        MaximumDelay = TimeSpan.FromSeconds(30),
                        MaximumRetries = 5
                    }
                });

            _processorClient.ProcessEventAsync += OnEventReceivedHandler;

            _processorClient.ProcessErrorAsync += OnErrorCaughtHandler;

            await _processorClient.StartProcessingAsync(cancellationToken);
        }

        private async Task OnEventReceivedHandler(ProcessEventArgs args)
        {
            try
            {
                var messageBody = args.Data.EventBody.ToObjectFromJson<WeatherEntity>();
                var entity = JsonSerializer.Deserialize<WeatherEntity>(args.Data.Body.Span);
                if (entity is not null)
                {
                    await ProcessMessageAsync(entity, args.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from Event Hub on partition {PartitionId}", args.Partition.PartitionId);
                await ProcessErrorAsync(ex, args.CancellationToken);
            }
            finally
            {
                await UpdateCheckPointAsync(args);
            }
        }

        private async Task OnErrorCaughtHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing Event Hub messages on partition {PartitionId}", args.PartitionId);
            await ProcessErrorAsync(args.Exception, args.CancellationToken);
        }

        private async Task UpdateCheckPointAsync(ProcessEventArgs args)
        {
            try
            {
                //Also add retry logic here if needed, depending on the application's requirements.
                await args.UpdateCheckpointAsync(args.CancellationToken);
                _logger.LogInformation("Checkpoint updated for partition {PartitionId}", args.Partition.PartitionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checkpoint for partition {PartitionId}", args.Partition.PartitionId);
                //We can choose to handle the exception or rethrow it based on the application's needs. For now, we log the error and continue processing.
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processorClient is not null)
            {
                await _processorClient.StopProcessingAsync(cancellationToken);
            }
        }
    }
}