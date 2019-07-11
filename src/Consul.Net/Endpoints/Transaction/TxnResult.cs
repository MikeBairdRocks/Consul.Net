using Consul.Net.Endpoints.KV;

namespace Consul.Net.Endpoints.Transaction
{
  internal class TxnResult
  {
    public KVPair KV { get; set; }
  }
}