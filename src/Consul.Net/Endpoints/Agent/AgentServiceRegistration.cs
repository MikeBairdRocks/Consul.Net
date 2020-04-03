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

    /// <summary>
    /// The kind of service. Defaults to "" which is a typical Consul service.
    /// This value may also be "connect-proxy" for services that are Connect-capable proxies representing another service or "mesh-gateway" for instances of a mesh gateway
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Kind { get; set; }

    /// <summary>
    /// Specifies the configuration for a Connect proxy instance. This is only valid if Kind == "connect-proxy" or Kind == "mesh-gateway".
    /// See the <a href="https://www.consul.io/docs/connect/registration/service-registration.html">Proxy documentation</a> for full details.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public AgentServiceProxy Proxy { get; set; }

    /// <summary>
    ///   <para>
    ///     Specifies weights for the service.
    ///     Please see the service documentation for more information about weights. If this field is not provided weights will default to {"Passing": 1, "Warning": 1}.
    ///   </para>
    ///   <para>
    ///     It is important to note that this applies only to the locally registered service.
    ///     If you have multiple nodes all registering the same service their EnableTagOverride configuration and all other service configuration items are independent of one another.
    ///     Updating the tags for the service registered on one node is independent of the same service (by name) registered on another node.
    ///     If EnableTagOverride is not specified the default value is false. See anti-entropy syncs for more info.
    ///   </para>
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Weight { get; set; }
  }
}