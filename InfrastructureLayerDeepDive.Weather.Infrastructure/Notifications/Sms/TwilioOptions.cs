namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Sms
{
    public class TwilioOptions
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromPhoneNumber { get; set; } = string.Empty;
    }
}