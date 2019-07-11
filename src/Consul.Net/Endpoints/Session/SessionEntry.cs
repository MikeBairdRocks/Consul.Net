using System;
using System.Collections.Generic;
using Consul.Net.Utilities;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Session
{
  public class SessionEntry
  {
    public ulong CreateIndex { get; set; }

    public string ID { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Node { get; set; }

    public List<string> Checks { get; set; }

    [JsonConverter(typeof(NanoSecTimespanConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan? LockDelay { get; set; }

    [JsonConverter(typeof(SessionBehaviorConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public SessionBehavior Behavior { get; set; }

    [JsonConverter(typeof(DurationTimespanConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan? TTL { get; set; }

    public SessionEntry()
    {
      Checks = new List<string>();
    }

    public bool ShouldSerializeID()
    {
      return false;
    }

    public bool ShouldSerializeCreateIndex()
    {
      return false;
    }

    public bool ShouldSerializeChecks()
    {
      return Checks != null && Checks.Count != 0;
    }
  }
}