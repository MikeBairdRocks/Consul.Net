using System;

namespace Consul.Net.Endpoints.Health
{
  /// <summary>
  /// The status of a health check
  /// </summary>
  public class HealthStatus : IEquatable<HealthStatus>
  {
    private static readonly HealthStatus passing = new HealthStatus() { Status = "passing" };
    private static readonly HealthStatus warning = new HealthStatus() { Status = "warning" };
    private static readonly HealthStatus critical = new HealthStatus() { Status = "critical" };
    private static readonly HealthStatus maintenance = new HealthStatus() { Status = "maintenance" };
    private static readonly HealthStatus any = new HealthStatus() { Status = "any" };

    public const string NodeMaintenance = "_node_maintenance";
    public const string ServiceMaintenancePrefix = "_service_maintenance:";

    public string Status { get; private set; }

    public static HealthStatus Passing
    {
      get { return passing; }
    }

    public static HealthStatus Warning
    {
      get { return warning; }
    }

    public static HealthStatus Critical
    {
      get { return critical; }
    }

    public static HealthStatus Maintenance
    {
      get { return maintenance; }
    }

    public static HealthStatus Any
    {
      get { return any; }
    }

    public bool Equals(HealthStatus other)
    {
      return other != null && ReferenceEquals(this, other);
    }

    public override bool Equals(object other)
    {
      // other could be a reference type, the is operator will return false if null
      return other is HealthStatus && Equals(other as HealthStatus);
    }

    public override int GetHashCode()
    {
      return Status.GetHashCode();
    }
  }
}