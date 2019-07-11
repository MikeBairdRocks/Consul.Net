using System.Collections.Generic;

namespace Consul.Net.Endpoints.Health
{
  public static class HealthCheckExtension
  {
    public static HealthStatus AggregatedStatus(this IEnumerable<HealthCheck> checks)
    {
      if (checks == null)
      {
        return HealthStatus.Passing;
      }

      bool warning = false, critical = false, maintenance = false;
      foreach (var check in checks)
      {
        if (!string.IsNullOrEmpty(check.CheckID) &&
            (check.CheckID == HealthStatus.NodeMaintenance || check.CheckID.StartsWith(HealthStatus.ServiceMaintenancePrefix)))
        {
          maintenance = true;
          break;
        }
        else if (check.Status == HealthStatus.Critical)
        {
          critical = true;
        }
        else if (check.Status == HealthStatus.Warning)
        {
          warning = true;
        }
      }

      if (maintenance)
      {
        return HealthStatus.Maintenance;
      }
      else if (critical)
      {
        return HealthStatus.Critical;
      }
      else if (warning)
      {
        return HealthStatus.Warning;
      }
      else
      {
        return HealthStatus.Passing;
      }
    }
  }
}