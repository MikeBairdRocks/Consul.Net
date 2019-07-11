using Consul.Net.Endpoints.KV;

namespace Consul.Net.Endpoints.Transaction
{
  internal class TxnOp
  {
    public KVTxnOp KV { get; set; }
  }
}