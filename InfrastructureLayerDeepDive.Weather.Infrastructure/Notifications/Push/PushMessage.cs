namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Push
{
    public class PushMessage
    {
        public required string DeviceToken { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
        public IDictionary<string, string>? Data { get; set; }
    }
}