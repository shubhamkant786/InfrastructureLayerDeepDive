using System.Text.Json;
using global::Azure.Messaging.ServiceBus;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Application.Models;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.ServiceBus
{
    public class ServiceBusWeatherConsumer : IMessageConsumer<WeatherModel>, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly ILogger<ServiceBusWeatherConsumer> _logger;

        public ServiceBusWeatherConsumer(IOptions<ServiceBusOptions> options, ILogger<ServiceBusWeatherConsumer> logger)
        {
            _logger = logger;
            _client = new ServiceBusClient(options.Value.ConnectionString);
            _processor = _client.CreateProcessor(options.Value.QueueName, new ServiceBusProcessorOptions
            {
                Identifier = "WeatherConsumer",
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                MaxAutoLockRenewalDuration = Timeout.InfiniteTimeSpan
            });
        }

        public async Task StartAsync(Func<WeatherModel, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken)
        {
            _processor.ProcessMessageAsync += async args =>
            {
                var entity = JsonSerializer.Deserialize<WeatherModel>(args.Message.Body.ToArray());
                if (entity is not null)
                {
                    await onMessageReceived(entity, cancellationToken);
                }
                await args.CompleteMessageAsync(args.Message, cancellationToken);
            };

            _processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Error processing Service Bus message from {EntityPath}", args.EntityPath);
                return Task.CompletedTask;
            };

            await _processor.StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}