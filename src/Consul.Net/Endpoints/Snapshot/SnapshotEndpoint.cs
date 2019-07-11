﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;

namespace Consul.Net.Endpoints.Snapshot
{
  public interface ISnapshotEndpoint
  {
    Task<QueryResult<Stream>> Save(CancellationToken ct = default(CancellationToken));
    Task<QueryResult<Stream>> Save(QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Restore(Stream s, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> Restore(Stream s, WriteOptions q, CancellationToken ct = default(CancellationToken));
  }
  
  public class SnapshotEndpoint : ISnapshotEndpoint
  {
    private readonly ConsulClient _client;

    /// <summary>
    /// Snapshot can be used to query the /v1/snapshot endpoint to take snapshots of
    /// Consul's internal state and restore snapshots for disaster recovery.
    /// </summary>
    /// <param name="c"></param>
    internal SnapshotEndpoint(ConsulClient c)
    {
      _client = c;
    }

    public Task<WriteResult> Restore(Stream s, CancellationToken ct = default(CancellationToken))
    {
      return Restore(s, WriteOptions.Default, ct);
    }

    public Task<WriteResult> Restore(Stream s, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Put("/v1/snapshot", s, q).Execute(ct);
    }

    public Task<QueryResult<Stream>> Save(CancellationToken ct = default(CancellationToken))
    {
      return Save(QueryOptions.Default, ct);
    }

    public Task<QueryResult<Stream>> Save(QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<Stream>("/v1/snapshot", q).ExecuteStreaming(ct);
    }
  }
}