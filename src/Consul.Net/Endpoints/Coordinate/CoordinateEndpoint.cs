using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;

namespace Consul.Net.Endpoints.Coordinate
{
  public interface ICoordinateEndpoint
  {
    Task<QueryResult<CoordinateDatacenterMap[]>> Datacenters(CancellationToken ct = default);
    Task<QueryResult<CoordinateEntry[]>> Nodes(CancellationToken ct = default);
    Task<QueryResult<CoordinateEntry[]>> Nodes(QueryOptions q, CancellationToken ct = default);
  }
  
  // May want to rework this as Dictionary<string,List<CoordinateEntry>>

  public class CoordinateEndpoint : ICoordinateEndpoint
  {
    private readonly ConsulClient _client;

    internal CoordinateEndpoint(ConsulClient c)
    {
      _client = c;
    }

    /// <summary>
    /// Datacenters is used to return the coordinates of all the servers in the WAN pool.
    /// </summary>
    /// <returns>A query result containing a map of datacenters, each with a list of coordinates of all the servers in the WAN pool</returns>
    public Task<QueryResult<CoordinateDatacenterMap[]>> Datacenters(CancellationToken ct = default)
    {
      return _client.Get<CoordinateDatacenterMap[]>(string.Format("/v1/coordinate/datacenters")).Execute(ct);
    }

    /// <summary>
    /// Nodes is used to return the coordinates of all the nodes in the LAN pool.
    /// </summary>
    /// <returns>A query result containing coordinates of all the nodes in the LAN pool</returns>
    public Task<QueryResult<CoordinateEntry[]>> Nodes(CancellationToken ct = default)
    {
      return Nodes(QueryOptions.Default, ct);
    }

    /// <summary>
    /// Nodes is used to return the coordinates of all the nodes in the LAN pool.
    /// </summary>
    /// <param name="q">Customized query options</param>
    /// <returns>A query result containing coordinates of all the nodes in the LAN pool</returns>
    public Task<QueryResult<CoordinateEntry[]>> Nodes(QueryOptions q, CancellationToken ct = default)
    {
      return _client.Get<CoordinateEntry[]>(string.Format("/v1/coordinate/nodes"), q).Execute(ct);
    }
  }
}
