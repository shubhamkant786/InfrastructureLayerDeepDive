using System.Net.Http.Headers;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.TokenGenerator;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.GraphqlDependency
{
    public class GraphqlClientFactory(IHttpClientFactory httpClientFactory
        , IOptions<GraphqlClientOptions> options
        , ITokenGenerator tokenGenerator)
    {
        public async Task<IGraphQLClient> CreateClient()
        {
            var httpClient = httpClientFactory.CreateClient(nameof(GraphqlClientFactory));

            if (options.Value.IsBasicAuthenticationEnabled && !string.IsNullOrWhiteSpace(options.Value.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", options.Value.ApiKey);
            }
            else if (options.Value.IsBasicAuthenticationEnabled)
            {
                var token = await tokenGenerator.AuthenticateAndGenerateToken();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var graphQlOptions = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(options.Value.EndpointUrl)
            };

            return new GraphQLHttpClient(graphQlOptions, new SystemTextJsonSerializer(), httpClient);
        }
    }
}