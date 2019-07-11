using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Transaction
{
  public class TxnError
  {
    [JsonProperty]
    public int OpIndex { get; private set; }
    [JsonProperty]
    public string What { get; private set; }
  }
}