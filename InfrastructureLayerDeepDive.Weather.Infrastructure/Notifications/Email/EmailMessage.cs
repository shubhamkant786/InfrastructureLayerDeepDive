namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Email
{
    public class EmailMessage
    {
        public required string ToAddress { get; set; }
        public required string CcAddress { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
        public bool IsHtml { get; set; } = true;
        public IReadOnlyCollection<(string FileName, Stream FileStream)> Attachments;
    }
}