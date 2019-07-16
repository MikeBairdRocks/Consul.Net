using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Query
{
  public interface IPreparedQueryEndpoint
  {
    Task<WriteResult<string>> Create(PreparedQueryDefinition query, CancellationToken ct = default);
    Task<WriteResult<string>> Create(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default);
    Task<WriteResult> Update(PreparedQueryDefinition query, CancellationToken ct = default);
    Task<WriteResult> Update(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default);
    Task<QueryResult<PreparedQueryDefinition[]>> List(CancellationToken ct = default);
    Task<QueryResult<PreparedQueryDefinition[]>> List(QueryOptions q, CancellationToken ct = default);
    Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, CancellationToken ct = default);
    Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, QueryOptions q, CancellationToken ct = default);
    Task<WriteResult> Delete(string queryID, CancellationToken ct = default);
    Task<WriteResult> Delete(string queryID, WriteOptions q, CancellationToken ct = default);
    Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, CancellationToken ct = default);
    Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, QueryOptions q, CancellationToken ct = default);
  }
  
  public class PreparedQueryEndpoint : IPreparedQueryEndpoint
  {
    private class PreparedQueryCreationResult
    {
      [JsonProperty] internal string ID { get; set; }
    }

    private readonly ConsulClient _client;

    internal PreparedQueryEndpoint(ConsulClient c)
    {
      _client = c;
    }

    public Task<WriteResult<string>> Create(PreparedQueryDefinition query, CancellationToken ct = default)
    {
      return Create(query, WriteOptions.Default, ct);
    }

    public async Task<WriteResult<string>> Create(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default)
    {
      var res = await _client.Post<PreparedQueryDefinition, PreparedQueryCreationResult>("/v1/query", query, q).Execute(ct).ConfigureAwait(false);
      return new WriteResult<string>(res, res.Response.ID);
    }

    public Task<WriteResult> Delete(string queryID, CancellationToken ct = default)
    {
      return Delete(queryID, WriteOptions.Default, ct);
    }

    public async Task<WriteResult> Delete(string queryID, WriteOptions q, CancellationToken ct = default)
    {
      var res = await _client.DeleteReturning<string>($"/v1/query/{queryID}", q).Execute(ct);
      return new WriteResult(res);
    }

    public Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, CancellationToken ct = default)
    {
      return Execute(queryIDOrName, QueryOptions.Default, ct);
    }

    public Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, QueryOptions q, CancellationToken ct = default)
    {
      return _client.Get<PreparedQueryExecuteResponse>($"/v1/query/{queryIDOrName}/execute", q).Execute(ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, CancellationToken ct = default)
    {
      return Get(queryID, QueryOptions.Default, ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, QueryOptions q, CancellationToken ct = default)
    {
      return _client.Get<PreparedQueryDefinition[]>($"/v1/query/{queryID}", q).Execute(ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> List(CancellationToken ct = default)
    {
      return List(QueryOptions.Default, ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> List(QueryOptions q, CancellationToken ct = default)
    {
      return _client.Get<PreparedQueryDefinition[]>("/v1/query", q).Execute(ct);
    }

    public Task<WriteResult> Update(PreparedQueryDefinition query, CancellationToken ct = default)
    {
      return Update(query, WriteOptions.Default, ct);
    }

    public Task<WriteResult> Update(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default)
    {
      return _client.Put($"/v1/query/{query.ID}", query, q).Execute(ct);
    }
  }
}