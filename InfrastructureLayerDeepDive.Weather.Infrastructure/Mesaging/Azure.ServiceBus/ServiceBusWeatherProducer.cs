using System.Text.Json;
using global::Azure.Messaging.ServiceBus;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.ServiceBus
{
    public class ServiceBusWeatherProducer : IMessageProducer<WeatherEntity>, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public ServiceBusWeatherProducer(IOptions<ServiceBusOptions> options)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);
            _sender = _client.CreateSender(options.Value.QueueName);
        }

        public async Task PublishAsync(WeatherEntity message, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(message);
            var serviceBusMessage = new ServiceBusMessage(payload)
            {
                ContentType = "application/json",
                Subject = nameof(WeatherEntity)
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}