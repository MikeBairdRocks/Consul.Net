﻿using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;

namespace Consul.Net.Endpoints.Health
{
  public interface IHealthEndpoint
  {
    Task<QueryResult<HealthCheck[]>> Checks(string service, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<HealthCheck[]>> Checks(string service, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<HealthCheck[]>> Node(string node, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<HealthCheck[]>> Node(string node, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ServiceEntry[]>> Service(string service, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ServiceEntry[]>> Service(string service, string tag, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ServiceEntry[]>> Service(string service, string tag, bool passingOnly, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<ServiceEntry[]>> Service(string service, string tag, bool passingOnly, QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<HealthCheck[]>> State(HealthStatus status, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<HealthCheck[]>> State(HealthStatus status, QueryOptions q, CancellationToken ct = default(CancellationToken));
  }
  
  /// <summary>
  /// Health can be used to query the Health endpoints
  /// </summary>
  public class HealthEndpoint : IHealthEndpoint
  {
    private readonly ConsulClient _client;

    internal HealthEndpoint(ConsulClient c)
    {
      _client = c;
    }

    /// <summary>
    /// Checks is used to return the checks associated with a service
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <returns>A query result containing the health checks matching the provided service ID, or a query result with a null response if no service matched the provided ID</returns>
    public Task<QueryResult<HealthCheck[]>> Checks(string service, CancellationToken ct = default(CancellationToken))
    {
      return Checks(service, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Checks is used to return the checks associated with a service
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing the health checks matching the provided service ID, or a query result with a null response if no service matched the provided ID</returns>
    public Task<QueryResult<HealthCheck[]>> Checks(string service, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<HealthCheck[]>(string.Format("/v1/health/checks/{0}", service), q).Execute(ct);
    }

    /// <summary>
    /// Node is used to query for checks belonging to a given node
    /// </summary>
    /// <param name="node">The node name</param>
    /// <returns>A query result containing the health checks matching the provided node ID, or a query result with a null response if no node matched the provided ID</returns>
    public Task<QueryResult<HealthCheck[]>> Node(string node, CancellationToken ct = default(CancellationToken))
    {
      return Node(node, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Node is used to query for checks belonging to a given node
    /// </summary>
    /// <param name="node">The node name</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing the health checks matching the provided node ID, or a query result with a null response if no node matched the provided ID</returns>
    public Task<QueryResult<HealthCheck[]>> Node(string node, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<HealthCheck[]>(string.Format("/v1/health/node/{0}", node), q).Execute(ct);
    }

    /// <summary>
    /// Service is used to query health information along with service info for a given service. It can optionally do server-side filtering on a tag or nodes with passing health checks only.
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <returns>A query result containing the service members matching the provided service ID, or a query result with a null response if no service members matched the filters provided</returns>
    public Task<QueryResult<ServiceEntry[]>> Service(string service, CancellationToken ct = default(CancellationToken))
    {
      return Service(service, string.Empty, false, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Service is used to query health information along with service info for a given service. It can optionally do server-side filtering on a tag or nodes with passing health checks only.
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <param name="tag">The service member tag</param>
    /// <returns>A query result containing the service members matching the provided service ID and tag, or a query result with a null response if no service members matched the filters provided</returns>
    public Task<QueryResult<ServiceEntry[]>> Service(string service, string tag, CancellationToken ct = default(CancellationToken))
    {
      return Service(service, tag, false, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Service is used to query health information along with service info for a given service. It can optionally do server-side filtering on a tag or nodes with passing health checks only.
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <param name="tag">The service member tag</param>
    /// <param name="passingOnly">Only return if the health check is in the Passing state</param>
    /// <returns>A query result containing the service members matching the provided service ID, tag, and health status, or a query result with a null response if no service members matched the filters provided</returns>
    public Task<QueryResult<ServiceEntry[]>> Service(string service, string tag, bool passingOnly, CancellationToken ct = default(CancellationToken))
    {
      return Service(service, tag, passingOnly, QueryOptions.Default, ct);
    }

    /// <summary>
    /// Service is used to query health information along with service info for a given service. It can optionally do server-side filtering on a tag or nodes with passing health checks only.
    /// </summary>
    /// <param name="service">The service ID</param>
    /// <param name="tag">The service member tag</param>
    /// <param name="passingOnly">Only return if the health check is in the Passing state</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing the service members matching the provided service ID, tag, and health status, or a query result with a null response if no service members matched the filters provided</returns>
    public Task<QueryResult<ServiceEntry[]>> Service(string service, string tag, bool passingOnly, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      var req = _client.Get<ServiceEntry[]>(string.Format("/v1/health/service/{0}", service), q);
      if (!string.IsNullOrEmpty(tag))
      {
        req.Params["tag"] = tag;
      }
      if (passingOnly)
      {
        req.Params["passing"] = "1";
      }
      return req.Execute(ct);
    }

    /// <summary>
    /// State is used to retrieve all the checks in a given state. The wildcard "any" state can also be used for all checks.
    /// </summary>
    /// <param name="status">The health status to filter for</param>
    /// <returns>A query result containing a list of health checks in the specified state, or a query result with a null response if no health checks matched the provided state</returns>
    public Task<QueryResult<HealthCheck[]>> State(HealthStatus status, CancellationToken ct = default(CancellationToken))
    {
      return State(status, QueryOptions.Default, ct);
    }

    /// <summary>
    /// // State is used to retrieve all the checks in a given state. The wildcard "any" state can also be used for all checks.
    /// </summary>
    /// <param name="status">The health status to filter for</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>A query result containing a list of health checks in the specified state, or a query result with a null response if no health checks matched the provided state</returns>
    public Task<QueryResult<HealthCheck[]>> State(HealthStatus status, QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<HealthCheck[]>(string.Format("/v1/health/state/{0}", status.Status), q).Execute(ct);
    }
  }
}