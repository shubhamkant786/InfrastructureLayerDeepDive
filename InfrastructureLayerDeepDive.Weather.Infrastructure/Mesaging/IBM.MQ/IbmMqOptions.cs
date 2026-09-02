namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.IBM.MQ
{
    public class IbmMqOptions
    {
        public string QueueManager { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public bool IsAcknowledge { get; set; } = false;
    }
}