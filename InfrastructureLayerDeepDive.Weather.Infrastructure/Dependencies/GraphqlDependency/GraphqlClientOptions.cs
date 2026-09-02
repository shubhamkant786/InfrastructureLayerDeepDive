namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.GraphqlDependency
{
    public class GraphqlClientOptions
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public bool IsBasicAuthenticationEnabled { get; set; } = false;
        public string? ApiKey { get; set; }
    }
}