using Azure;
using global::Azure.Messaging.EventHubs;
using global::Azure.Messaging.EventHubs.Producer;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Azure.EventHubs
{
    public class EventHubWeatherProducer : IMessageProducer<WeatherEntity>, IAsyncDisposable
    {
        private readonly EventHubProducerClient _producerClient;
        private readonly ILogger<EventHubWeatherProducer> _logger;

        public EventHubWeatherProducer(IOptions<EventHubOptions> options, ILogger<EventHubWeatherProducer> logger)
        {
            _logger = logger;
            var isAuthenticated = false;
            var sasToken = CreateToken($"https://{options.Value.ConnectionString}/{options.Value.EventHubName}" +
                $"/publishers/{options.Value.Publisher}", options.Value.KeyName!, options.Value.KeyValue!);

            
            _producerClient = isAuthenticated? new EventHubProducerClient(
                               options.Value.ConnectionString,
                               $"{options.Value.EventHubName}/publishers/{options.Value.Publisher}",
                               new AzureSasCredential(sasToken))
            : new EventHubProducerClient(options.Value.ConnectionString, options.Value.EventHubName);
        }

        private static string CreateToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var week = 60 * 60 * 24 * 7;
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + week);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }

        public async Task PublishAsync(WeatherEntity message, CancellationToken cancellationToken)
        {
            var eventHubProps = await _producerClient.GetEventHubPropertiesAsync(cancellationToken);
            
            using var eventBatch = await _producerClient.CreateBatchAsync(cancellationToken);

            var payload = JsonSerializer.SerializeToUtf8Bytes(message);
            var eventData = new EventData(payload);

            if (!eventBatch.TryAdd(eventData))
            {
                _logger.LogWarning("Event data too large for batch. Date: {Date}", message.Date);
                throw new InvalidOperationException("Event data too large to fit in batch.");
            }

            await _producerClient.SendAsync(eventBatch, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _producerClient.DisposeAsync();
        }
    }
}