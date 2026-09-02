using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Sms
{
    public class TwilioSmsNotificationService : INotificationService<SmsMessage>
    {
        private readonly TwilioOptions _options;
        private readonly ILogger<TwilioSmsNotificationService> _logger;

        public TwilioSmsNotificationService(IOptions<TwilioOptions> options, ILogger<TwilioSmsNotificationService> logger)
        {
            _options = options.Value;
            _logger = logger;
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        }

        public async Task SendAsync(SmsMessage message, CancellationToken cancellationToken)
        {
            //var call = CallResource.Create(
            //    new PhoneNumber(message.ToPhoneNumber),
            //    from: new PhoneNumber(_options.FromPhoneNumber),
            //    url: new Uri("https://my.twiml.here")
            //);
            var result = await MessageResource.CreateAsync(
                body: message.Body,
                from: new PhoneNumber(_options.FromPhoneNumber),
                to: new PhoneNumber(message.ToPhoneNumber));

            _logger.LogInformation("SMS sent to {ToPhoneNumber} with status {Status}, SID {Sid}",
                message.ToPhoneNumber, result.Status, result.Sid);
        }
    }
}