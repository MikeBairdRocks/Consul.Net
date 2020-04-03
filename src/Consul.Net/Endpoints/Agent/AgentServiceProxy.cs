using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Agent
{
  /// <summary>
  /// Specifies the configuration for a Connect proxy instance. This is only valid if Kind == "connect-proxy" or Kind == "mesh-gateway".
  /// See the <a href="https://www.consul.io/docs/connect/registration/service-registration.html">Proxy documentation</a> for full details.
  /// </summary>
  public class AgentServiceProxy
  {
    /// <summary>
    /// Specifies the name of the service this instance is proxying.
    /// Both side-car and centralized load-balancing proxies must specify this.
    /// It is used during service discovery to find the correct proxy instances to route to for a given service name.
    /// </summary>
    public string DestinationServiceName { get; set; }
    
    /// <summary>
    /// Specifies the ID of a single specific service instance that this proxy is representing.
    /// This is only valid for side-car style proxies that run on the same node.
    /// It is assumed that the service instance is registered via the same Consul agent so the ID is unique and has no node qualifier.
    /// This is useful to show in tooling which proxy instance is a side-car for which application instance and will enable fine-grained analysis of the metrics coming from the proxy.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DestinationServiceId { get; set; }
    
    /// <summary>
    /// Specifies the address a side-car proxy should attempt to connect to the local application instance on. Defaults to 127.0.0.1.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string LocalServiceAddress { get; set; }
    
    /// <summary>
    /// Specifies the port a side-car proxy should attempt to connect to the local application instance on.
    /// Defaults to the port advertised by the service instance identified by destination_service_id if it exists otherwise it may be empty in responses.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? LocalServicePort { get; set; }
    
    /// <summary>
    /// Specifies opaque config JSON that will be stored and returned along with the service instance from future API calls.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Config { get; set; }
    
    /// <summary>
    /// Specifies the upstream services this proxy should create listeners for.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public AgentServiceUpstream[] Upstreams { get; set; }
    
    /// <summary>
    /// Specifies the mesh gateway configuration for this proxy. The format is defined in the <a href="https://www.consul.io/docs/connect/registration/service-registration.html#mesh-gateway-configuration-reference">Mesh Gateway Configuration Reference</a>.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string MeshGateway { get; set; }
  }
}