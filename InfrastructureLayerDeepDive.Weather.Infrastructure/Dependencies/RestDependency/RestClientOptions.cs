namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.RestDependency
{
    public class RestClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        //Basic service level
        public string? ApiKeyName { get; set; }
        public string? ApiKeyValue { get; set; }
        //Bearer user level
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool IsCertificateValidationEnabled { get; set; } = false;
        //Certificate service level
        public string CertificateName { get; set; } = string.Empty;
        public string CertificatePassword { get; set; }
    }
}