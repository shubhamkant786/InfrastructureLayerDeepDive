using System.Collections;
using System.Text;
using System.Text.Json;
using IBM.WMQ;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.IBM.MQ
{
    public class IbmMqWeatherProducer : IMessageProducer<WeatherEntity>, IDisposable
    {
        private readonly MQQueueManager _queueManager;
        private readonly IbmMqOptions _options;

        public IbmMqWeatherProducer(IOptions<IbmMqOptions> options)
        {
            _options = options.Value;

            var properties = new Hashtable
            {
                { MQC.CHANNEL_PROPERTY, _options.Channel },
                { MQC.CONNECTION_NAME_PROPERTY, _options.ConnectionName },
                { MQC.USER_ID_PROPERTY, _options.UserId },
                { MQC.PASSWORD_PROPERTY, _options.Password }
            };

            _queueManager = new MQQueueManager(_options.QueueManager, properties);
        }

        public Task PublishAsync(WeatherEntity message, CancellationToken cancellationToken)
        {
            using var queue = _queueManager.AccessQueue(_options.QueueName, MQC.MQOO_OUTPUT);

            var mqMessage = new MQMessage();
            var payload = JsonSerializer.Serialize(message);
            mqMessage.WriteString(payload);

            var putMessageOptions = new MQPutMessageOptions();
            queue.Put(mqMessage, putMessageOptions);
            queue.Close();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _queueManager.Disconnect();
            _queueManager.Close();
        }
    }
}