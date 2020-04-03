using System.Collections.Generic;

namespace Consul.Net.Endpoints.Agent
{
  /// <summary>
  /// AgentService represents a service known to the agent
  /// </summary>
  public class AgentService
  {
    /// <summary>
    /// Specifies a unique ID for this service. This must be unique per agent. This defaults to the Name parameter if not provided.
    /// </summary>
    public string ID { get; set; }

    public string Service { get; set; }
    public string[] Tags { get; set; }
    public int Port { get; set; }
    public string Address { get; set; }
    public bool EnableTagOverride { get; set; }
    public IDictionary<string, string> Meta { get; set; }
  }
}