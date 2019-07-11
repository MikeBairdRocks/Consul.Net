using Newtonsoft.Json;

namespace Consul.Net.Endpoints.ACL
{
  /// <summary>
  /// ACLEntry is used to represent an ACL entry
  /// </summary>
  public class ACLEntry
  {
    public ulong CreateIndex { get; set; }
    public ulong ModifyIndex { get; set; }

    public string ID { get; set; }
    public string Name { get; set; }

    [JsonConverter(typeof(ACLTypeConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ACLType Type { get; set; }

    public string Rules { get; set; }

    public bool ShouldSerializeCreateIndex()
    {
      return false;
    }

    public bool ShouldSerializeModifyIndex()
    {
      return false;
    }

    public ACLEntry()
      : this(string.Empty, string.Empty, string.Empty)
    {
    }

    public ACLEntry(string name, string rules)
      : this(string.Empty, name, rules)
    {
    }

    public ACLEntry(string id, string name, string rules)
    {
      Type = ACLType.Client;
      ID = id;
      Name = name;
      Rules = rules;
    }
  }
}