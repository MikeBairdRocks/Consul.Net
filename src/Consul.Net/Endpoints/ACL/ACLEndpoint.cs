using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.ACL
{
  public interface IACLEndpoint
  {
    Task<WriteResult<string>> Create(ACLToken acl, CancellationToken token = default);
    Task<WriteResult<string>> Create(ACLToken acl, WriteOptions options, CancellationToken token = default);
    Task<QueryResult<ACLToken>> Get(string id, CancellationToken token = default);
    Task<QueryResult<ACLToken>> Get(string id, QueryOptions options, CancellationToken token = default);
    Task<QueryResult<ACLToken>> Self(CancellationToken token = default);
    Task<QueryResult<ACLToken>> Self(QueryOptions options, CancellationToken token = default);
    Task<WriteResult> Update(ACLToken acl, CancellationToken token = default);
    Task<WriteResult> Update(ACLToken acl, WriteOptions options, CancellationToken token = default);
    Task<WriteResult<string>> Clone(string id, CancellationToken token = default);
    Task<WriteResult<string>> Clone(string id, WriteOptions options, CancellationToken token = default);
    Task<WriteResult<bool>> Delete(string id, CancellationToken token = default);
    Task<WriteResult<bool>> Delete(string id, WriteOptions options, CancellationToken token = default);
    Task<QueryResult<ACLToken[]>> List(CancellationToken token = default);
    Task<QueryResult<ACLToken[]>> List(QueryOptions options, CancellationToken token = default);

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
    
    /// <summary>
    /// Create is used to generate a new token with the given parameters
    /// </summary>
    /// <param name="acl">The ACL token to create</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public Task<WriteResult<string>> Create(ACLToken acl, CancellationToken token = default)
    {
      return Create(acl, WriteOptions.Default, token);
    }

    /// <summary>
    /// Create is used to generate a new token with the given parameters
    /// </summary>
    /// <param name="acl">The ACL token to create</param>
    /// <param name="options">Customized write options</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public async Task<WriteResult<string>> Create(ACLToken acl, WriteOptions options, CancellationToken token = default)
    {
      var result = await _client.Put<ACLToken, ACLToken>("/v1/acl/token", acl, options).Execute(token).ConfigureAwait(false);
      return new WriteResult<string>(result, result.Response.AccessorID);
    }

    /// <summary>
    /// Info is used to query for information about an ACL token
    /// </summary>
    /// <param name="id">The ACL ID to request information about</param>
    /// <returns>A query result containing the ACL entry matching the provided ID, or a query result with a null response if no token matched the provided ID</returns>
    public Task<QueryResult<ACLToken>> Get(string id, CancellationToken token = default)
    {
      return Get(id, QueryOptions.Default, token);
    }

    /// <summary>
    /// Info is used to query for information about an ACL token
    /// </summary>
    /// <param name="id">The ACL ID to request information about</param>
    /// <param name="options">Customized query options</param>
    /// <param name="token">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing the ACL entry matching the provided ID, or a query result with a null response if no token matched the provided ID</returns>
    public async Task<QueryResult<ACLToken>> Get(string id, QueryOptions options, CancellationToken token = default)
    {
      var result = await _client.Get<ACLToken>($"/v1/acl/token/{id}", options).Execute(token).ConfigureAwait(false);
      return result;
    }
    
    /// <summary>
    /// Info is used to query for information about an ACL token
    /// </summary>
    /// <param name="token">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing the ACL entry matching the provided ID, or a query result with a null response if no token matched the provided ID</returns>
    public Task<QueryResult<ACLToken>> Self(CancellationToken token = default)
    {
      return Self(QueryOptions.Default, token);
    }
    
    /// <summary>
    /// Info is used to query for information about an ACL token
    /// </summary>
    /// <param name="options">Customized query options</param>
    /// <param name="token">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing the ACL entry matching the provided ID, or a query result with a null response if no token matched the provided ID</returns>
    public async Task<QueryResult<ACLToken>> Self(QueryOptions options, CancellationToken token = default)
    {
      var result = await _client.Get<ACLToken>($"/v1/acl/token/self", options).Execute(token).ConfigureAwait(false);
      return result;
    }
    
    /// <summary>
    /// Update is used to update the rules of an existing token
    /// </summary>
    /// <param name="acl">The ACL token to update</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Update(ACLToken acl, CancellationToken token = default)
    {
      return Update(acl, WriteOptions.Default, token);
    }

    /// <summary>
    /// Update is used to update the rules of an existing token
    /// </summary>
    /// <param name="acl">The ACL token to update</param>
    /// <param name="options">Customized write options</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Update(ACLToken acl, WriteOptions options, CancellationToken token = default)
    {
      return _client.Put($"/v1/acl/token/{acl.AccessorID}", acl, options).Execute(token);
    }

    /// <summary>
    /// Clone is used to return a new token cloned from an existing one
    /// </summary>
    /// <param name="id">The ACL ID to clone</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public Task<WriteResult<string>> Clone(string id, CancellationToken token = default)
    {
      return Clone(id, WriteOptions.Default, token);
    }

    /// <summary>
    /// Clone is used to return a new token cloned from an existing one
    /// </summary>
    /// <param name="id">The ACL ID to clone</param>
    /// <param name="options">Customized write options</param>
    /// <returns>A write result containing the newly created ACL token</returns>
    public async Task<WriteResult<string>> Clone(string id, WriteOptions options, CancellationToken token = default)
    {
      var result = await _client.PutReturning<ACLToken>($"/v1/acl/token/{id}/clone", options).Execute(token).ConfigureAwait(false);
      return new WriteResult<string>(result, result.Response.AccessorID);
    }
    
    /// <summary>
    /// Delete is used to delete a given ACL token ID
    /// </summary>
    /// <param name="id">The ACL ID to destroy</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult<bool>> Delete(string id, CancellationToken token = default)
    {
      return Delete(id, WriteOptions.Default, token);
    }

    /// <summary>
    /// Delete is used to destroy a given ACL token ID
    /// </summary>
    /// <param name="id">The ACL ID to destroy</param>
    /// <param name="options">Customized write options</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult<bool>> Delete(string id, WriteOptions options, CancellationToken token = default)
    {
      return _client.PutReturning<bool>($"/v1/acl/destroy/{id}", options).Execute(token);
    }

    /// <summary>
    /// List is used to get all the ACL tokens
    /// </summary>
    /// <returns>A write result containing the list of all ACLs</returns>
    public Task<QueryResult<ACLToken[]>> List(CancellationToken token = default)
    {
      return List(QueryOptions.Default, token);
    }

    /// <summary>
    /// List is used to get all the ACL tokens
    /// </summary>
    /// <param name="options">Customized query options</param>
    /// <param name="token">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A write result containing the list of all ACLs</returns>
    public Task<QueryResult<ACLToken[]>> List(QueryOptions options, CancellationToken token = default)
    {
      return _client.Get<ACLToken[]>("/v1/acl/tokens", options).Execute(token);
    }
  }
}