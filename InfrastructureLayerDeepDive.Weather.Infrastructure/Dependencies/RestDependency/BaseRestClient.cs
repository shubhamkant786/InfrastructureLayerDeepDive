using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.RestDependency
{
    public class BaseRestClient<T>(ILogger<BaseRestClient<T>> logger
        , IHttpClientFactory httpClientFactory
        , ITokenAcquisition tokenAquisition) where T : class
    {
        private readonly bool _useBearerToken = true;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public async Task<IEnumerable<T>?> GetAsync(string baseUrl
            , string endpointUrl
            , IEnumerable<KeyValuePair<string, string>>? queryStrings
            , Dictionary<string, string>? customHeaders
            , CancellationToken cancellationToken)
        {
            // Implement the logic to make a GET request to the specified URL
            // and return the response deserialized into an instance of T.
            var httpClient = httpClientFactory.CreateClient("Weather");
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "WeatherApp");

            var queryString = queryStrings != null ? "?" + string.Join("&", queryStrings
                .Select(q => $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value)}")) : string.Empty;
            endpointUrl = endpointUrl + queryString;
            var endpointUri = new Uri(endpointUrl + queryString, UriKind.Relative);
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
            customHeaders?.ToList().ForEach(header => httpRequest.Headers.Add(header.Key, header.Value));
            if (_useBearerToken)
            {
                var accessToken = await tokenAquisition.GetAccessTokenForAppAsync("appscope");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            
            var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                string payload = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Error response from {BaseUrl}{EndpointUrl}: {StatusCode} - {Payload}", baseUrl, endpointUrl, httpResponse.StatusCode, payload);
                //handle error response, throw exception or return default value
                return default;
            }

            string text = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(text))
                return null;

            return JsonSerializer.Deserialize<IEnumerable<T>>(text, _jsonSerializerOptions);
        }
    }
}
