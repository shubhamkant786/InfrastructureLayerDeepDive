using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.TokenGenerator
{
    public class TokenGenerator(IHttpClientFactory httpClientFactory
        , IOptions<IDPOptions> options): ITokenGenerator
    {
        private readonly IDPOptions _options = options.Value;
        public async Task<string> AuthenticateAndGenerateToken()
        {
            var httpClient = httpClientFactory.CreateClient();
            var base64Encoding = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Encoding);

            var bodyParams = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };

            var content = new FormUrlEncodedContent(bodyParams);

            var httpResponse = await httpClient.PostAsync(_options.TokenEndpoint, content);

            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException hex)
            {
                throw new IDPException("Error when try to connect to api manager", hex);
            }

            await using var contentStream = await httpResponse.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<IDPTokenDto>(contentStream, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (result == null)
            {
                throw new IDPException("Empty response from the Api Manager authentication endpoint");
            }

            return result.Access_token!;
        }
    }
}
