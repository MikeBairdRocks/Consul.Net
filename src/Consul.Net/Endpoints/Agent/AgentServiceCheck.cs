using System;
using Consul.Net.Endpoints.Health;
using Consul.Net.Utilities;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Agent
{
  /// <summary>
  /// AgentServiceCheck is used to create an associated check for a service
  /// </summary>
  public class AgentServiceCheck
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Script { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DockerContainerID { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Shell { get; set; } // Only supported for Docker.

    [JsonConverter(typeof(DurationTimespanConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan? Interval { get; set; }

    [JsonConverter(typeof(DurationTimespanConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan? Timeout { get; set; }

    [JsonConverter(typeof(DurationTimespanConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan? TTL { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string HTTP { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string TCP { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(HealthStatusConverter))]
    public HealthStatus Status { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool TLSSkipVerify { get; set; }

    /// <summary>
    /// In Consul 0.7 and later, checks that are associated with a service
    /// may also contain this optional DeregisterCriticalServiceAfter field,
    /// which is a timeout in the same Go time format as Interval and TTL. If
    /// a check is in the critical state for more than this configured value,
    /// then its associated service (and all of its associated checks) will
    /// automatically be deregistered.
    /// </summary>
    [JsonConverter(typeof(DurationTimespanConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan? DeregisterCriticalServiceAfter { get; set; }
  }
}