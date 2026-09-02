namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Sms
{
    public class SmsMessage
    {
        public required string ToPhoneNumber { get; set; }
        public required string Body { get; set; }
    }
}