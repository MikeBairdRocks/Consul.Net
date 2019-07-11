using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;

namespace Consul.Net.Endpoints
{
  public interface IRawEndpoint
  {
    Task<QueryResult<dynamic>> Query(string endpoint, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult<dynamic>> Write(string endpoint, object obj, WriteOptions q, CancellationToken ct = default(CancellationToken));
  }
  
  /// <summary>
  /// Raw can be used to do raw queries against custom endpoints
  /// </summary>
  public class RawEndpoint : IRawEndpoint
  {
    private readonly ConsulClient _client;

    internal RawEndpoint(ConsulClient c)
    {
      _client = c;
    }

    /// <summary>
    /// Query is used to do a GET request against an endpoint and deserialize the response into an interface using standard Consul conventions.
    /// </summary>
    /// <param name="endpoint">The URL endpoint to access</param>
    /// <param name="q">Custom query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>The data returned by the custom endpoint</returns>
    public Task<QueryResult<dynamic>> Query(string endpoint, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<dynamic>(endpoint, q).Execute(ct);
    }

    /// <summary>
    /// Write is used to do a PUT request against an endpoint and serialize/deserialized using the standard Consul conventions.
    /// </summary>
    /// <param name="endpoint">The URL endpoint to access</param>
    /// <param name="obj">The object to serialize and send to the endpoint. Must be able to be JSON serialized, or be an object of type byte[], which is sent without serialzation.</param>
    /// <param name="q">Custom write options</param>
    /// <returns>The data returned by the custom endpoint in response to the write request</returns>
    public Task<WriteResult<dynamic>> Write(string endpoint, object obj, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Put<object, dynamic>(endpoint, obj, q).Execute(ct);
    }
  }
}