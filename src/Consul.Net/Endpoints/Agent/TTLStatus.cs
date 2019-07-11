using System;

namespace Consul.Net.Endpoints.Agent
{
  /// <summary>
  /// The status of a TTL check
  /// </summary>
  public class TTLStatus : IEquatable<TTLStatus>
  {
    private static readonly TTLStatus passingStatus = new TTLStatus() { Status = "passing", LegacyStatus = "pass" };
    private static readonly TTLStatus warningStatus = new TTLStatus() { Status = "warning", LegacyStatus = "warn" };
    private static readonly TTLStatus criticalStatus = new TTLStatus() { Status = "critical", LegacyStatus = "fail" };

    public string Status { get; private set; }
    internal string LegacyStatus { get; private set; }

    public static TTLStatus Pass
    {
      get { return passingStatus; }
    }

    public static TTLStatus Warn
    {
      get { return warningStatus; }
    }

    public static TTLStatus Critical
    {
      get { return criticalStatus; }
    }

    [Obsolete("Use TTLStatus.Critical instead. This status will be an error in 0.7.0+", true)]
    public static TTLStatus Fail
    {
      get { return criticalStatus; }
    }

    public bool Equals(TTLStatus other)
    {
      return other != null && ReferenceEquals(this, other);
    }

    public override bool Equals(object other)
    {
      // other could be a reference type, the is operator will return false if null
      return other is TTLStatus && Equals(other as TTLStatus);
    }

    public override int GetHashCode()
    {
      return Status.GetHashCode();
    }
  }
}