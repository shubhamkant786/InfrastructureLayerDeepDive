namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.RestDependency
{
    public class RestClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string? ApiKeyName { get; set; }
        public string? ApiKeyValue { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool IsCertificateValidationEnabled { get; set; } = false;
        public string CertificateName { get; set; } = string.Empty;
        public string CertificatePassword { get; set; }
    }
}