using Consul.Net.Endpoints.Agent;

namespace Consul.Net.Endpoints.Catalog
{
  public class CatalogRegistration
  {
    public string Node { get; set; }
    public string Address { get; set; }
    public string Datacenter { get; set; }
    public AgentService Service { get; set; }
    public AgentCheck Check { get; set; }
  }
}