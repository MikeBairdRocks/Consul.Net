using System.Collections.Generic;
using Consul.Net.Endpoints.Transaction;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.KV
{
  /// <summary>
  /// KVTxnResponse  is used to return the results of a transaction.
  /// </summary>
  public class KVTxnResponse
  {
    [JsonIgnore] public bool Success { get; internal set; }
    [JsonProperty] public List<TxnError> Errors { get; internal set; }
    [JsonProperty] public List<KVPair> Results { get; internal set; }

    public KVTxnResponse()
    {
      Results = new List<KVPair>();
      Errors = new List<TxnError>();
    }

    internal KVTxnResponse(TxnResponse txnRes)
    {
      if (txnRes == null)
      {
        Results = new List<KVPair>(0);
        Errors = new List<TxnError>(0);
        return;
      }

      if (txnRes.Results == null)
      {
        Results = new List<KVPair>(0);
      }
      else
      {
        Results = new List<KVPair>(txnRes.Results.Count);
        foreach (var txnResult in txnRes.Results)
        {
          Results.Add(txnResult.KV);
        }
      }

      if (txnRes.Errors == null)
      {
        Errors = new List<TxnError>(0);
      }
      else
      {
        Errors = txnRes.Errors;
      }
    }
  }
}