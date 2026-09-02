namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Mesaging.Kafka
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string ConsumerGroupId { get; set; } = string.Empty;
        public bool AutoCommit { get; set; } = false;
        public int AutoCommitIntervalMs { get; set; } = 5000;
        public int ManualCommitMessagesCount { get; set; } = 100;
        public int SessionTimeoutMs { get; set; } = 60000;

    }
}