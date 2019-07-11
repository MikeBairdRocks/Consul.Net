using Consul.Net.Endpoints.Agent;
using Consul.Net.Endpoints.Catalog;

namespace Consul.Net.Endpoints.Health
{
  /// <summary>
  /// ServiceEntry is used for the health service endpoint
  /// </summary>
  public class ServiceEntry
  {
    public Node Node { get; set; }
    public AgentService Service { get; set; }
    public HealthCheck[] Checks { get; set; }
  }
}