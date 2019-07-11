using System.Collections.Generic;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Query
{
  /// <summary>
  /// QueryDatacenterOptions sets options about how we fail over if there are no healthy nodes in the local datacenter.
  /// </summary>
  public class QueryDatacenterOptions
  {
    /// <summary>
    /// NearestN is set to the number of remote datacenters to try, based on network coordinates.
    /// </summary>
    public int NearestN { get; set; }

    /// <summary>
    /// Datacenters is a fixed list of datacenters to try after NearestN. We
    /// never try a datacenter multiple times, so those are subtracted from
    /// this list before proceeding.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Datacenters { get; set; }

    public QueryDatacenterOptions()
    {
      Datacenters = new List<string>();
    }
  }
}