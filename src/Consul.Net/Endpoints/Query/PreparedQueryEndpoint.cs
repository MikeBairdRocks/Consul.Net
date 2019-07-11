using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Query
{
  public interface IPreparedQueryEndpoint
  {
    Task<WriteResult<string>> Create(PreparedQueryDefinition query, CancellationToken ct = default(CancellationToken));
    Task<WriteResult<string>> Create(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Update(PreparedQueryDefinition query, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Update(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<PreparedQueryDefinition[]>> List(CancellationToken ct = default(CancellationToken));
    Task<QueryResult<PreparedQueryDefinition[]>> List(QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Delete(string queryID, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Delete(string queryID, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, QueryOptions q, CancellationToken ct = default(CancellationToken));
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

    public Task<WriteResult<string>> Create(PreparedQueryDefinition query, CancellationToken ct = default(CancellationToken))
    {
      return Create(query, WriteOptions.Default, ct);
    }

    public async Task<WriteResult<string>> Create(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      var res = await _client.Post<PreparedQueryDefinition, PreparedQueryCreationResult>("/v1/query", query, q).Execute(ct).ConfigureAwait(false);
      return new WriteResult<string>(res, res.Response.ID);
    }

    public Task<WriteResult> Delete(string queryID, CancellationToken ct = default(CancellationToken))
    {
      return Delete(queryID, WriteOptions.Default, ct);
    }

    public async Task<WriteResult> Delete(string queryID, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      var res = await _client.DeleteReturning<string>(string.Format("/v1/query/{0}", queryID), q).Execute(ct);
      return new WriteResult(res);
    }

    public Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, CancellationToken ct = default(CancellationToken))
    {
      return Execute(queryIDOrName, QueryOptions.Default, ct);
    }

    public Task<QueryResult<PreparedQueryExecuteResponse>> Execute(string queryIDOrName, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<PreparedQueryExecuteResponse>(string.Format("/v1/query/{0}/execute", queryIDOrName), q).Execute(ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, CancellationToken ct = default(CancellationToken))
    {
      return Get(queryID, QueryOptions.Default, ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> Get(string queryID, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<PreparedQueryDefinition[]>(string.Format("/v1/query/{0}", queryID), q).Execute(ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> List(CancellationToken ct = default(CancellationToken))
    {
      return List(QueryOptions.Default, ct);
    }

    public Task<QueryResult<PreparedQueryDefinition[]>> List(QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<PreparedQueryDefinition[]>("/v1/query", q).Execute(ct);
    }

    public Task<WriteResult> Update(PreparedQueryDefinition query, CancellationToken ct = default(CancellationToken))
    {
      return Update(query, WriteOptions.Default, ct);
    }

    public Task<WriteResult> Update(PreparedQueryDefinition query, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Put(string.Format("/v1/query/{0}", query.ID), query, q).Execute(ct);
    }
  }
}