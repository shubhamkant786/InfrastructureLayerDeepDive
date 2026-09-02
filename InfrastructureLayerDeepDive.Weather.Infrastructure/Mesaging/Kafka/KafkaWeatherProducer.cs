using System.Text.Json;
using Confluent.Kafka;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Kafka
{
    public class KafkaWeatherProducer : IMessageProducer<WeatherEntity>, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaWeatherProducer(IOptions<KafkaOptions> options)
        {
            _topic = options.Value.Topic;
            var config = new ProducerConfig { BootstrapServers = options.Value.BootstrapServers };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync(WeatherEntity message, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = message.Date.ToString("O"),
                Value = payload
            };

            await _producer.ProduceAsync(_topic, kafkaMessage, cancellationToken);
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}