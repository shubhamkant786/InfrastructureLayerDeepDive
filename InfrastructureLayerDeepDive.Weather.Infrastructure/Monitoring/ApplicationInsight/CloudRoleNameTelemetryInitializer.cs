

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Monitoring.ApplicationInsight
{
    public class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _cloudRoleName;
        public CloudRoleNameTelemetryInitializer(string cloudRoleName)
        {
            _cloudRoleName = cloudRoleName;
        }

        public void Initialize(ITelemetry telemetry) => telemetry.Context.Cloud.RoleName = _cloudRoleName;
    }
}
