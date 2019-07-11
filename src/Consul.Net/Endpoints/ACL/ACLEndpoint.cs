using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.ACL
{
  public interface IACLEndpoint
  {
    Task<WriteResult<string>> Clone(string id, CancellationToken ct = default(CancellationToken));
    Task<WriteResult<string>> Clone(string id, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult<string>> Create(ACLEntry acl, CancellationToken ct = default(CancellationToken));
    Task<WriteResult<string>> Create(ACLEntry acl, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult<bool>> Destroy(string id, CancellationToken ct = default(CancellationToken));
    Task<WriteResult<bool>> Destroy(string id, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ACLEntry>> Info(string id, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ACLEntry>> Info(string id, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ACLEntry[]>> List(CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ACLEntry[]>> List(QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Update(ACLEntry acl, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Update(ACLEntry acl, WriteOptions q, CancellationToken ct = default(CancellationToken));
  }

  /// <summary>
  /// ACL can be used to query the ACL endpoints
  /// </summary>
  public class ACLEndpoint : IACLEndpoint
  {
    private readonly ConsulClient _client;

    internal ACLEndpoint(ConsulClient c)
    {
      _client = c;
    }

    private class ACLCreationResult
    {
      [JsonProperty]
      internal string ID { get; set; }
    }

    /// <summary>
    /// Create is used to generate a new token with the given parameters
    /// </summary>
    /// <param name="acl">The ACL entry to create</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public Task<WriteResult<string>> Create(ACLEntry acl, CancellationToken ct = default(CancellationToken))
    {
      return Create(acl, WriteOptions.Default, ct);
    }

    /// <summary>
    /// Create is used to generate a new token with the given parameters
    /// </summary>
    /// <param name="acl">The ACL entry to create</param>
    /// <param name="q">Customized write options</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public async Task<WriteResult<string>> Create(ACLEntry acl, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      var res = await _client.Put<ACLEntry, ACLCreationResult>("/v1/acl/create", acl, q).Execute(ct).ConfigureAwait(false);
      return new WriteResult<string>(res, res.Response.ID);
    }

    /// <summary>
    /// Update is used to update the rules of an existing token
    /// </summary>
    /// <param name="acl">The ACL entry to update</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Update(ACLEntry acl, CancellationToken ct = default(CancellationToken))
    {
      return Update(acl, WriteOptions.Default, ct);
    }

    /// <summary>
    /// Update is used to update the rules of an existing token
    /// </summary>
    /// <param name="acl">The ACL entry to update</param>
    /// <param name="q">Customized write options</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Update(ACLEntry acl, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Put("/v1/acl/update", acl, q).Execute(ct);
    }

    /// <summary>
    /// Destroy is used to destroy a given ACL token ID
    /// </summary>
    /// <param name="id">The ACL ID to destroy</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult<bool>> Destroy(string id, CancellationToken ct = default(CancellationToken))
    {
      return Destroy(id, WriteOptions.Default, ct);
    }

    /// <summary>
    /// Destroy is used to destroy a given ACL token ID
    /// </summary>
    /// <param name="id">The ACL ID to destroy</param>
    /// <param name="q">Customized write options</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult<bool>> Destroy(string id, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.PutReturning<bool>(string.Format("/v1/acl/destroy/{0}", id), q).Execute(ct);
    }

    /// <summary>
    /// Clone is used to return a new token cloned from an existing one
    /// </summary>
    /// <param name="id">The ACL ID to clone</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public Task<WriteResult<string>> Clone(string id, CancellationToken ct = default(CancellationToken))
    {
      return Clone(id, WriteOptions.Default, ct);
    }

    /// <summary>
    /// Clone is used to return a new token cloned from an existing one
    /// </summary>
    /// <param name="id">The ACL ID to clone</param>
    /// <param name="q">Customized write options</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public async Task<WriteResult<string>> Clone(string id, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      var res = await _client.PutReturning<ACLCreationResult>(string.Format("/v1/acl/clone/{0}", id), q).Execute(ct).ConfigureAwait(false);
      return new WriteResult<string>(res, res.Response.ID);
    }

    /// <summary>
    /// Info is used to query for information about an ACL token
    /// </summary>
    /// <param name="id">The ACL ID to request information about</param>
    /// <returns>A query result containing the ACL entry matching the provided ID, or a query result with a null response if no token matched the provided ID</returns>
    public Task<QueryResult<ACLEntry>> Info(string id, CancellationToken ct = default(CancellationToken))
    {
      return Info(id, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Info is used to query for information about an ACL token
    /// </summary>
    /// <param name="id">The ACL ID to request information about</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing the ACL entry matching the provided ID, or a query result with a null response if no token matched the provided ID</returns>
    public async Task<QueryResult<ACLEntry>> Info(string id, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      var res = await _client.Get<ACLEntry[]>(string.Format("/v1/acl/info/{0}", id), q).Execute(ct).ConfigureAwait(false);
      return new QueryResult<ACLEntry>(res, res.Response != null && res.Response.Length > 0 ? res.Response[0] : null);
    }

    /// <summary>
    /// List is used to get all the ACL tokens
    /// </summary>
    /// <returns>A write result containing the list of all ACLs</returns>
    public Task<QueryResult<ACLEntry[]>> List(CancellationToken ct = default(CancellationToken))
    {
      return List(QueryOptions.Default, ct);
    }

    /// <summary>
    /// List is used to get all the ACL tokens
    /// </summary>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A write result containing the list of all ACLs</returns>
    public Task<QueryResult<ACLEntry[]>> List(QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<ACLEntry[]>("/v1/acl/list", q).Execute(ct);
    }
  }
}