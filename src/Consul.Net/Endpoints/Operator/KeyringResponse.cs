using System.Collections.Generic;

namespace Consul.Net.Endpoints.Operator
{
  /// <summary>
  /// KeyringResponse is returned when listing the gossip encryption keys
  /// </summary>
  public class KeyringResponse
  {
    /// <summary>
    /// Whether this response is for a WAN ring
    /// </summary>
    public bool WAN { get; set; }
    /// <summary>
    /// The datacenter name this request corresponds to
    /// </summary>
    public string Datacenter { get; set; }
    /// <summary>
    /// A map of the encryption keys to the number of nodes they're installed on
    /// </summary>
    public IDictionary<string, int> Keys { get; set; }
    /// <summary>
    /// The total number of nodes in this ring
    /// </summary>
    public int NumNodes { get; set; }
  }
}