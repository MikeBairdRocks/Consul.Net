using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;

namespace Consul.Net.Endpoints.Catalog
{
  public interface ICatalogEndpoint
  {
    Task<QueryResult<string[]>> Datacenters(CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Deregister(CatalogDeregistration reg, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Deregister(CatalogDeregistration reg, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<CatalogNode>> Node(string node, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<CatalogNode>> Node(string node, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<Node[]>> Nodes(CancellationToken ct = default(CancellationToken));
    Task<QueryResult<Node[]>> Nodes(QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Register(CatalogRegistration reg, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Register(CatalogRegistration reg, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<CatalogService[]>> Service(string service, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<CatalogService[]>> Service(string service, string tag, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<CatalogService[]>> Service(string service, string tag, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<Dictionary<string, string[]>>> Services(CancellationToken ct = default(CancellationToken));
    Task<QueryResult<Dictionary<string, string[]>>> Services(QueryOptions q, CancellationToken ct = default(CancellationToken));
  }

  /// <summary>
  /// Catalog can be used to query the Catalog endpoints
  /// </summary>
  public class CatalogEndpoint : ICatalogEndpoint
  {
    private readonly ConsulClient _client;

    internal CatalogEndpoint(ConsulClient c)
    {
      _client = c;
    }

    /// <summary>
    /// Register a new catalog item
    /// </summary>
    /// <param name="reg">A catalog registration</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Register(CatalogRegistration reg, CancellationToken ct = default(CancellationToken))
    {
      return Register(reg, WriteOptions.Default, ct);
    }

    /// <summary>
    /// Register a new catalog item
    /// </summary>
    /// <param name="reg">A catalog registration</param>
    /// <param name="q">Customized write options</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Register(CatalogRegistration reg, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Put("/v1/catalog/register", reg, q).Execute(ct);
    }

    /// <summary>
    /// Deregister an existing catalog item
    /// </summary>
    /// <param name="reg">A catalog deregistration</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Deregister(CatalogDeregistration reg, CancellationToken ct = default(CancellationToken))
    {
      return Deregister(reg, WriteOptions.Default, ct);
    }

    /// <summary>
    /// Deregister an existing catalog item
    /// </summary>
    /// <param name="reg">A catalog deregistration</param>
    /// <param name="q">Customized write options</param>
    /// <returns>An empty write result</returns>
    public Task<WriteResult> Deregister(CatalogDeregistration reg, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Put("/v1/catalog/deregister", reg, q).Execute(ct);
    }

    /// <summary>
    /// Datacenters is used to query for all the known datacenters
    /// </summary>
    /// <returns>A list of datacenter names</returns>
    public Task<QueryResult<string[]>> Datacenters(CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<string[]>("/v1/catalog/datacenters").Execute(ct);
    }

    /// <summary>
    /// Nodes is used to query all the known nodes
    /// </summary>
    /// <returns>A list of all nodes</returns>
    public Task<QueryResult<Node[]>> Nodes(CancellationToken ct = default(CancellationToken))
    {
      return Nodes(QueryOptions.Default, ct);
    }

    /// <summary>
    /// Nodes is used to query all the known nodes
    /// </summary>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A list of all nodes</returns>
    public Task<QueryResult<Node[]>> Nodes(QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<Node[]>("/v1/catalog/nodes", q).Execute(ct);
    }

    /// <summary>
    /// Services is used to query for all known services
    /// </summary>
    /// <returns>A list of all services</returns>
    public Task<QueryResult<Dictionary<string, string[]>>> Services(CancellationToken ct = default(CancellationToken))
    {
      return Services(QueryOptions.Default, ct);
    }

    /// <summary>
    /// Services is used to query for all known services
    /// </summary>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A list of all services</returns>
    public Task<QueryResult<Dictionary<string, string[]>>> Services(QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<Dictionary<string, string[]>>("/v1/catalog/services", q).Execute(ct);
    }

    /// <summary>
    /// Service is used to query catalog entries for a given service
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A list of service instances</returns>
    public Task<QueryResult<CatalogService[]>> Service(string service, CancellationToken ct = default(CancellationToken))
    {
      return Service(service, string.Empty, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Service is used to query catalog entries for a given service
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <param name="tag">A tag to filter on</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A list of service instances</returns>
    public Task<QueryResult<CatalogService[]>> Service(string service, string tag, CancellationToken ct = default(CancellationToken))
    {
      return Service(service, tag, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Service is used to query catalog entries for a given service
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <param name="tag">A tag to filter on</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A list of service instances</returns>
    public Task<QueryResult<CatalogService[]>> Service(string service, string tag, QueryOptions q, CancellationToken ct)
    {
      var req = _client.Get<CatalogService[]>(string.Format("/v1/catalog/service/{0}", service), q);
      if (!string.IsNullOrEmpty(tag))
      {
        req.Params["tag"] = tag;
      }
      return req.Execute(ct);
    }

    /// <summary>
    /// Node is used to query for service information about a single node
    /// </summary>
    /// <param name="node">The node name</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>The node information including a list of services</returns>
    public Task<QueryResult<CatalogNode>> Node(string node, CancellationToken ct = default(CancellationToken))
    {
      return Node(node, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Node is used to query for service information about a single node
    /// </summary>
    /// <param name="node">The node name</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>The node information including a list of services</returns>
    public Task<QueryResult<CatalogNode>> Node(string node, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<CatalogNode>(string.Format("/v1/catalog/node/{0}", node), q).Execute(ct);
    }
  }
}
