using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Agent
{
  public class AgentServiceUpstream
  {
    /// <summary>
    /// Specifies the name of the service or prepared query to route connect to.
    /// The prepared query should be the name or the ID of the prepared query. 
    /// </summary>
    public string DestinationName { get; set; }
    
    /// <summary>
    /// Specifies the type of discovery query to use to find an instance to connect to.
    /// Valid values are service or prepared_query. Defaults to service.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DestinationType { get; set; }
    
    /// <summary>
    /// Specifies the address to bind a local listener to for the application to make outbound connections to this upstream. Defaults to 127.0.0.1. 
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string LocalBindAddress  { get; set; }
    
    /// <summary>
    /// Specifies the port to bind a local listener to for the application to make outbound connections to this upstream
    /// </summary>
    public int LocalBindPort { get; set; }
    
    /// <summary>
    /// Specifies the datacenter to issue the discovery query too. Defaults to the local datacenter. 
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Datacenter { get; set; }
    
    /// <summary>
    /// Specifies opaque configuration options that will be provided to the proxy instance for this specific upstream.
    /// Can contain any valid JSON object. This might be used to configure proxy-specific features like timeouts or retries for the given upstream.
    /// See the <a href="https://www.consul.io/docs/connect/configuration.html#built-in-proxy-options">built-in proxy configuration</a> reference for options available when using the built-in proxy.
    /// If using Envoy as a proxy, see <a href="https://www.consul.io/docs/connect/configuration.html#envoy-options">Envoy configuration reference</a>
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Config { get; set; }
    
    /// <summary>
    /// Specifies the mesh gateway configuration for this proxy. The format is defined in the <a href="https://www.consul.io/docs/connect/registration/service-registration.html#mesh-gateway-configuration-reference">Mesh Gateway Configuration Reference</a>. 
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string MeshGateway { get; set; }
  }
}