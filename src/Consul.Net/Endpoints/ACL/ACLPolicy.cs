using Newtonsoft.Json;

namespace Consul.Net.Endpoints.ACL
{
  public class ACLPolicy
  {
    public ACLPolicy()
      : this(string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }

    public ACLPolicy(string name, string rules, string description = "")
      : this(string.Empty, name, rules, description)
    {
    }

    public ACLPolicy(string id, string name, string rules, string description = "")
    {
      ID = id;
      Name = name;
      Rules = rules;
      Description = description;
    }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ID { get; set; }
    
    /// <summary>
    /// Specifies a name for the ACL policy. The name can contain alphanumeric characters, dashes -, and underscores _. This name must be unique.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Free form human readable description of the policy.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }
    
    /// <summary>
    /// Specifies rules for the ACL policy.
    /// The format of the Rules property is detailed in the ACL <see href="https://www.consul.io/docs/acl/acl-rules.html">Rules documentation</see>.
    /// </summary>
    public string Rules { get; set; }
    
    /// <summary>
    /// Specifies the datacenters the policy is valid within. When no datacenters are provided the policy is valid in all datacenters including those which do not yet exist but may in the future.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string[] Datacenters { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Hash { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ulong CreateIndex { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ulong ModifyIndex { get; set; }
  }
}