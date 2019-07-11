using System.Collections.Generic;

namespace Consul.Net.Endpoints.Operator
{
  /// <summary>
  /// RaftConfigration is returned when querying for the current Raft configuration.
  /// </summary>
  public class RaftConfiguration
  {
    /// <summary>
    /// Servers has the list of servers in the Raft configuration.
    /// </summary>
    public List<RaftServer> Servers { get; set; }

    /// <summary>
    /// Index has the Raft index of this configuration.
    /// </summary>
    public ulong Index { get; set; }
  }
}