using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Models;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Event
{
  public interface IEventEndpoint
  {
    Task<WriteResult<string>> Fire(UserEvent ue, CancellationToken ct = default);
    Task<WriteResult<string>> Fire(UserEvent ue, WriteOptions q, CancellationToken ct = default);
    ulong IDToIndex(string uuid);
    Task<QueryResult<UserEvent[]>> List(CancellationToken ct = default);
    Task<QueryResult<UserEvent[]>> List(string name, CancellationToken ct = default);
    Task<QueryResult<UserEvent[]>> List(string name, QueryOptions q, CancellationToken ct = default);
  }
  
  public class EventEndpoint : IEventEndpoint
  {
    private class EventCreationResult
    {
      [JsonProperty]
      internal string ID { get; set; }
    }

    private readonly ConsulClient _client;

    internal EventEndpoint(ConsulClient c)
    {
      _client = c;
    }

    public Task<WriteResult<string>> Fire(UserEvent ue, CancellationToken ct = default)
    {
      return Fire(ue, WriteOptions.Default, ct);
    }

    /// <summary>
    /// Fire is used to fire a new user event. Only the Name, Payload and Filters are respected. This returns the ID or an associated error. Cross DC requests are supported.
    /// </summary>
    /// <param name="ue">A User Event definition</param>
    /// <param name="q">Customized write options</param>
    /// <returns></returns>
    public async Task<WriteResult<string>> Fire(UserEvent ue, WriteOptions q, CancellationToken ct = default)
    {
      var req = _client.Put<byte[], EventCreationResult>($"/v1/event/fire/{ue.Name}", ue.Payload, q);
      if (!string.IsNullOrEmpty(ue.NodeFilter))
      {
        req.Params["node"] = ue.NodeFilter;
      }
      if (!string.IsNullOrEmpty(ue.ServiceFilter))
      {
        req.Params["service"] = ue.ServiceFilter;
      }
      if (!string.IsNullOrEmpty(ue.TagFilter))
      {
        req.Params["tag"] = ue.TagFilter;
      }
      var res = await req.Execute(ct).ConfigureAwait(false);
      return new WriteResult<string>(res, res.Response.ID);
    }

    /// <summary>
    /// List is used to get the most recent events an agent has received. This list can be optionally filtered by the name. This endpoint supports quasi-blocking queries. The index is not monotonic, nor does it provide provide LastContact or KnownLeader.
    /// </summary>
    /// <returns>An array of events</returns>
    public Task<QueryResult<UserEvent[]>> List(CancellationToken ct = default)
    {
      return List(string.Empty, QueryOptions.Default, ct);
    }

    /// <summary>
    /// List is used to get the most recent events an agent has received. This list can be optionally filtered by the name. This endpoint supports quasi-blocking queries. The index is not monotonic, nor does it provide provide LastContact or KnownLeader.
    /// </summary>
    /// <param name="name">The name of the event to filter for</param>
    /// <returns>An array of events</returns>
    public Task<QueryResult<UserEvent[]>> List(string name, CancellationToken ct = default)
    {
      return List(name, QueryOptions.Default, ct);
    }

    /// <summary>
    /// List is used to get the most recent events an agent has received. This list can be optionally filtered by the name. This endpoint supports quasi-blocking queries. The index is not monotonic, nor does it provide provide LastContact or KnownLeader.
    /// </summary>
    /// <param name="name">The name of the event to filter for</param>
    /// <param name="q">Customized query options</param>
    /// <param name="ct">Cancellation token for long poll request. If set, OperationCanceledException will be thrown if the request is cancelled before completing</param>
    /// <returns>An array of events</returns>
    public Task<QueryResult<UserEvent[]>> List(string name, QueryOptions q, CancellationToken ct)
    {
      var req = _client.Get<UserEvent[]>("/v1/event/list", q);
      if (!string.IsNullOrEmpty(name))
      {
        req.Params["name"] = name;
      }
      return req.Execute(ct);
    }

    /// <summary>
    /// IDToIndex is a bit of a hack. This simulates the index generation to convert an event ID into a WaitIndex.
    /// </summary>
    /// <param name="uuid">The Event UUID</param>
    /// <returns>A "wait index" generated from the UUID</returns>
    public ulong IDToIndex(string uuid)
    {
      var lower = uuid.Take(8).Concat(uuid.Skip(9).Take(4)).Concat(uuid.Skip(14).Take(4)).ToArray();
      var upper = uuid.Skip(19).Take(4).Concat(uuid.Skip(24).Take(12)).ToArray();
      var lowVal = ulong.Parse(new string(lower), NumberStyles.HexNumber);
      var highVal = ulong.Parse(new string(upper), NumberStyles.HexNumber);
      return lowVal ^ highVal;
    }
  }
}