using System.Threading;
using System.Threading.Tasks;

namespace Consul.Net.Endpoints.Status
{
  public interface IStatusEndpoint
  {
    Task<string> Leader(CancellationToken ct = default(CancellationToken));
    Task<string[]> Peers(CancellationToken ct = default(CancellationToken));
  }
  
  public class StatusEndpoint : IStatusEndpoint
  {
    private readonly ConsulClient _client;

    internal StatusEndpoint(ConsulClient c)
    {
      _client = c;
    }

    /// <summary>
    /// Leader is used to query for a known leader
    /// </summary>
    /// <returns>A write result containing the leader node name</returns>
    public async Task<string> Leader(CancellationToken ct = default(CancellationToken))
    {
      var res = await _client.Get<string>("/v1/status/leader").Execute(ct).ConfigureAwait(false);
      return res.Response;
    }

    /// <summary>
    /// Peers is used to query for a known raft peers
    /// </summary>
    /// <returns>A write result containing the list of Raft peers</returns>
    public async Task<string[]> Peers(CancellationToken ct = default(CancellationToken))
    {
      var res = await _client.Get<string[]>("/v1/status/peers").Execute(ct).ConfigureAwait(false);
      return res.Response;
    }
  }
}