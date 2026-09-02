using System.Text.Json;
using Confluent.Kafka;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Application.Models;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Kafka
{
    public class KafkaWeatherConsumer(
        IOptions<KafkaOptions> options
        , ILogger<KafkaWeatherConsumer> logger) : IMessageConsumer<WeatherModel>, IDisposable
    {
        private readonly IConsumer<string, string> _consumer
            = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = options.Value.ConsumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = options.Value.AutoCommit,
                SessionTimeoutMs = options.Value.SessionTimeoutMs, //if cosnusmer desn't send a heartbeat within this time, it will be considered as dead
                AutoCommitIntervalMs = options.Value.AutoCommitIntervalMs,
                HeartbeatIntervalMs = options.Value.SessionTimeoutMs / 3

            }).Build();
        private readonly KafkaOptions option = options.Value;

        private CancellationTokenSource? _internalCts;

        public async Task StartAsync(Func<WeatherModel, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken)
        {
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _consumer.Subscribe(option.Topic);


            while (!_internalCts.Token.IsCancellationRequested)
            {
                int messagesCount = 0;
                try
                {
                    var result = _consumer.Consume(_internalCts.Token);
                    var modelMessage = JsonSerializer.Deserialize<WeatherModel>(result.Message.Value);
                    if (modelMessage is not null)
                    {
                        await onMessageReceived(modelMessage, _internalCts.Token);
                    }
                    messagesCount++;

                    if (messagesCount >= option.ManualCommitMessagesCount && !option.AutoCommit)
                    {
                        _consumer.Commit(result);

                        messagesCount = 0;
                    }
                }
                catch (ConsumeException ex)
                {
                    if (ex.Error?.IsFatal == true)
                    {
                        logger.LogCritical(ex, "Fatal consume error (Code: {ErrorCode}, Reason: {Reason}). Consumer will be restarted.", ex.Error?.Code, ex.Error?.Reason);
                        throw;
                    }
                    //Consumer will try to reconnect internally.
                    logger.LogError(ex, "Recoverable Error when try to consume message from Kafka Queue, Consumer will try to reconnect automatically");
                }
                catch (OperationCanceledException)
                {
                    // Expected on shutdown.
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error consuming Kafka messages from topic {Topic}", option.Topic);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _internalCts?.Cancel();
            if (_consumer != null)
            {
                try
                {
                    logger.LogWarning("Closing Kafka consumer...");
                    _consumer.Close();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Exception during Kafka consumer close.");
                }
                try
                {
                    _consumer.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Exception during Kafka consumer dispose.");
                }
            }
            _consumer.Close();
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}