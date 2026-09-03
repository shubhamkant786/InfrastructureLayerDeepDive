using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
namespace BelgianRail.Atms.BFF.Application.Core.Utilities
{
    public class RequestBodyTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RequestBodyTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is RequestTelemetry requestTelemetry)
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null && context.Items.ContainsKey("RequestBody"))
                {
                    var requestBody = context.Items["RequestBody"] as string;
                    requestTelemetry.Properties["RequestBody"] = requestBody;
                }
            }
            else if (telemetry is DependencyTelemetry dependencyTelemetry)
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null && context.Items.ContainsKey("RequestBody"))
                {
                    var requestBody = context.Items["RequestBody"] as string;
                    dependencyTelemetry.Properties["RequestBody"] = requestBody;
                }
            }
        }
    }
}