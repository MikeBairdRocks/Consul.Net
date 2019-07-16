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

    public static TTLStatus Pass => passingStatus;

    public static TTLStatus Warn => warningStatus;

    public static TTLStatus Critical => criticalStatus;

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