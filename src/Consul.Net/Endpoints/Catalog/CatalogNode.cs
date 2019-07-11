using System.Collections.Generic;
using Consul.Net.Endpoints.Agent;

namespace Consul.Net.Endpoints.Catalog
{
  public class CatalogNode
  {
    public Node Node { get; set; }
    public Dictionary<string, AgentService> Services { get; set; }

    public CatalogNode()
    {
      Services = new Dictionary<string, AgentService>();
    }
  }
}