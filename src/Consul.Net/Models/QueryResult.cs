using System;

namespace Consul.Net.Models
{
  /// <summary>
  /// The result of a Consul API query
  /// </summary>
  public class QueryResult : ConsulResult
  {
    /// <summary>
    /// The index number when the query was serviced. This can be used as a WaitIndex to perform a blocking query
    /// </summary>
    public ulong LastIndex { get; set; }

    /// <summary>
    /// Time of last contact from the leader for the server servicing the request
    /// </summary>
    public TimeSpan LastContact { get; set; }

    /// <summary>
    /// Is there a known leader
    /// </summary>
    public bool KnownLeader { get; set; }

    /// <summary>
    /// Is address translation enabled for HTTP responses on this agent
    /// </summary>
    public bool AddressTranslationEnabled { get; set; }

    public QueryResult() { }
    public QueryResult(QueryResult other) : base(other)
    {
      LastIndex = other.LastIndex;
      LastContact = other.LastContact;
      KnownLeader = other.KnownLeader;
    }
  }

  /// <summary>
  /// The result of a Consul API query
  /// </summary>
  /// <typeparam name="T">Must be able to be deserialized from JSON</typeparam>
  public class QueryResult<T> : QueryResult
  {
    /// <summary>
    /// The result of the query
    /// </summary>
    public T Response { get; set; }
    public QueryResult() { }
    public QueryResult(QueryResult other) : base(other) { }
    public QueryResult(QueryResult other, T value) : base(other)
    {
      Response = value;
    }
  }
}