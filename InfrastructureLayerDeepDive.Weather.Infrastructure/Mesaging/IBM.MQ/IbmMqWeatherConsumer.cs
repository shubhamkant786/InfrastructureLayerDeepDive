using System.Collections;
using System.Text.Json;
using IBM.WMQ;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Application.Models;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.IBM.MQ
{
    public class IbmMqWeatherConsumer : IMessageConsumer<WeatherModel>, IDisposable
    {
        private readonly MQQueueManager _queueManager;
        private readonly IbmMqOptions _options;
        private readonly ILogger<IbmMqWeatherConsumer> _logger;
        private CancellationTokenSource? _internalCts;
        private Task? _consumeLoop;

        public IbmMqWeatherConsumer(IOptions<IbmMqOptions> options, ILogger<IbmMqWeatherConsumer> logger)
        {
            _options = options.Value;
            _logger = logger;

            var properties = new Hashtable
            {
                { MQC.CHANNEL_PROPERTY, _options.Channel },
                { MQC.CONNECTION_NAME_PROPERTY, _options.ConnectionName },
                { MQC.USER_ID_PROPERTY, _options.UserId },
                { MQC.PASSWORD_PROPERTY, _options.Password }
            };

            _queueManager = new MQQueueManager(_options.QueueManager, properties);
        }

        public Task StartAsync(Func<WeatherModel, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken)
        {
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _consumeLoop = Task.Run(async () =>
            {
                using var queue = _queueManager.AccessQueue(_options.QueueName, MQC.MQOO_INPUT_AS_Q_DEF);
                var getMessageOptions = new MQGetMessageOptions { WaitInterval = 5000, Options = MQC.MQGMO_WAIT };

                while (!_internalCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var mqMessage = new MQMessage();
                        queue.Get(mqMessage, getMessageOptions);

                        var payload = mqMessage.ReadString(mqMessage.MessageLength);
                        var entity = JsonSerializer.Deserialize<WeatherModel>(payload);
                        if (entity is not null)
                        {
                            await onMessageReceived(entity, _internalCts.Token);
                        }
                    }
                    catch (MQException mqEx) when (mqEx.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
                    {
                        // No message within wait interval; loop again.
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error consuming IBM MQ messages from queue {QueueName}", _options.QueueName);
                    }
                }
            }, _internalCts.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _internalCts?.Cancel();
            if (_consumeLoop is not null)
            {
                await _consumeLoop;
            }
        }

        public void Dispose()
        {
            _queueManager.Disconnect();
            _queueManager.Close();
        }
    }
}