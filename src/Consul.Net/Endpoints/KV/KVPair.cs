using Consul.Net.Utilities;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.KV
{
  /// <summary>
  /// KVPair is used to represent a single K/V entry
  /// </summary>
  [JsonConverter(typeof(KVPairConverter))]
  public class KVPair
  {
    public string Key { get; set; }

    public ulong CreateIndex { get; set; }
    public ulong ModifyIndex { get; set; }
    public ulong LockIndex { get; set; }
    public ulong Flags { get; set; }

    public byte[] Value { get; set; }
    public string Session { get; set; }

    public KVPair(string key)
    {
      Key = key;
    }

    internal KVPair()
    {
    }

    internal void Validate()
    {
      ValidatePath(Key);
    }

    static internal void ValidatePath(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        throw new InvalidKeyPairException("Invalid key. Key path is empty.");
      }
      else if (path[0] == '/')
      {
        throw new InvalidKeyPairException($"Invalid key. Key must not begin with a '/': {path}");
      }
    }
  }
}