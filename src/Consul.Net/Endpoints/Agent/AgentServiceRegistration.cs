using System.Collections.Generic;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Agent
{
  /// <summary>
  /// AgentServiceRegistration is used to register a new service
  /// </summary>
  public class AgentServiceRegistration
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ID { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string[] Tags { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int Port { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Address { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool EnableTagOverride { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public AgentServiceCheck Check { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public AgentServiceCheck[] Checks { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, string> Meta { get; set; }
  }
}