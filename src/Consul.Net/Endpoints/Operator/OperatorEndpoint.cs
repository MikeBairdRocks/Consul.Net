using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Operator
{
  public interface IOperatorEndpoint
  {
    Task<QueryResult<RaftConfiguration>> RaftGetConfiguration(CancellationToken ct = default(CancellationToken));
    Task<QueryResult<RaftConfiguration>> RaftGetConfiguration(QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> RaftRemovePeerByAddress(string address, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> RaftRemovePeerByAddress(string address, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> KeyringInstall(string key, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> KeyringInstall(string key, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<QueryResult<KeyringResponse[]>> KeyringList(CancellationToken ct = default(CancellationToken));
    Task<QueryResult<KeyringResponse[]>> KeyringList(QueryOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> KeyringRemove(string key, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> KeyringRemove(string key, WriteOptions q, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> KeyringUse(string key, CancellationToken ct = default(CancellationToken));
    Task<WriteResult> KeyringUse(string key, WriteOptions q, CancellationToken ct = default(CancellationToken));
  }
  
  public class OperatorEndpoint : IOperatorEndpoint
  {
    private readonly ConsulClient _client;

    /// <summary>
    /// Operator can be used to perform low-level operator tasks for Consul.
    /// </summary>
    /// <param name="c"></param>
    internal OperatorEndpoint(ConsulClient c)
    {
      _client = c;
    }

    /// <summary>
    /// KeyringRequest is used for performing Keyring operations
    /// </summary>
    private class KeyringRequest
    {
      [JsonProperty]
      internal string Key { get; set; }
    }

    /// <summary>
    /// RaftGetConfiguration is used to query the current Raft peer set.
    /// </summary>
    public Task<QueryResult<RaftConfiguration>> RaftGetConfiguration(CancellationToken ct = default(CancellationToken))
    {
      return RaftGetConfiguration(QueryOptions.Default, ct);
    }

    /// <summary>
    /// RaftGetConfiguration is used to query the current Raft peer set.
    /// </summary>
    public Task<QueryResult<RaftConfiguration>> RaftGetConfiguration(QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<RaftConfiguration>("/v1/operator/raft/configuration", q).Execute(ct);
    }

    /// <summary>
    /// RaftRemovePeerByAddress is used to kick a stale peer (one that it in the Raft
    /// quorum but no longer known to Serf or the catalog) by address in the form of
    /// "IP:port".
    /// </summary>
    public Task<WriteResult> RaftRemovePeerByAddress(string address, CancellationToken ct = default(CancellationToken))
    {
      return RaftRemovePeerByAddress(address, WriteOptions.Default, ct);
    }

    /// <summary>
    /// RaftRemovePeerByAddress is used to kick a stale peer (one that it in the Raft
    /// quorum but no longer known to Serf or the catalog) by address in the form of
    /// "IP:port".
    /// </summary>
    public Task<WriteResult> RaftRemovePeerByAddress(string address, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      var req = _client.Delete("/v1/operator/raft/peer", q);

      // From Consul repo:
      // TODO (slackpad) Currently we made address a query parameter. Once
      // IDs are in place this will be DELETE /v1/operator/raft/peer/<id>.
      req.Params["address"] = address;

      return req.Execute(ct);
    }

    /// <summary>
    /// KeyringInstall is used to install a new gossip encryption key into the cluster
    /// </summary>
    public Task<WriteResult> KeyringInstall(string key, CancellationToken ct = default(CancellationToken))
    {
      return KeyringInstall(key, WriteOptions.Default, ct);
    }

    /// <summary>
    /// KeyringInstall is used to install a new gossip encryption key into the cluster
    /// </summary>
    public Task<WriteResult> KeyringInstall(string key, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Post("/v1/operator/keyring", new KeyringRequest() { Key = key }, q).Execute(ct);
    }

    /// <summary>
    /// KeyringList is used to list the gossip keys installed in the cluster
    /// </summary>
    public Task<QueryResult<KeyringResponse[]>> KeyringList(CancellationToken ct = default(CancellationToken))
    {
      return KeyringList(QueryOptions.Default, ct);
    }

    /// <summary>
    /// KeyringList is used to list the gossip keys installed in the cluster
    /// </summary>
    public Task<QueryResult<KeyringResponse[]>> KeyringList(QueryOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Get<KeyringResponse[]>("/v1/operator/keyring", q).Execute(ct);
    }

    /// <summary>
    /// KeyringRemove is used to remove a gossip encryption key from the cluster
    /// </summary>
    public Task<WriteResult> KeyringRemove(string key, CancellationToken ct = default(CancellationToken))
    {
      return KeyringRemove(key, WriteOptions.Default, ct);
    }

    /// <summary>
    /// KeyringRemove is used to remove a gossip encryption key from the cluster
    /// </summary>
    public Task<WriteResult> KeyringRemove(string key, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.DeleteAccepting("/v1/operator/keyring", new KeyringRequest() { Key = key }, q).Execute(ct);
    }

    /// <summary>
    /// KeyringUse is used to change the active gossip encryption key
    /// </summary>
    public Task<WriteResult> KeyringUse(string key, CancellationToken ct = default(CancellationToken))
    {
      return KeyringUse(key, WriteOptions.Default, ct);
    }

    /// <summary>
    /// KeyringUse is used to change the active gossip encryption key
    /// </summary>
    public Task<WriteResult> KeyringUse(string key, WriteOptions q, CancellationToken ct = default(CancellationToken))
    {
      return _client.Put("/v1/operator/keyring", new KeyringRequest() { Key = key }, q).Execute(ct);
    }
  }
}
