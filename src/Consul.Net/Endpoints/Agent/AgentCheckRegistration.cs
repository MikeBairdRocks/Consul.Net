using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Agent
{
  /// <summary>
  /// AgentCheckRegistration is used to register a new check
  /// </summary>
  public class AgentCheckRegistration : AgentServiceCheck
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ID { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Notes { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ServiceID { get; set; }
  }
}