using System;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Query
{
  /// <summary>
  /// QueryDNSOptions controls settings when query results are served over DNS.
  /// </summary>
  public class QueryDNSOptions
  {
    /// <summary>
    /// TTL is the time to live for the served DNS results.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan? TTL { get; set; }
  }
}