using System;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.ACL
{
  /// <summary>
  /// ACLToken is used to represent an ACL token
  /// </summary>
  public class ACLToken
  {
    /// <summary>
    /// Specifies a UUID to use as the token's Accessor ID. If not specified a UUID will be generated for this field. Added in v1.5.0.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AccessorID { get; set; }

    /// <summary>
    /// Specifies a UUID to use as the token's Secret ID. If not specified a UUID will be generated for this field. Added in v1.5.0.
    /// Note: The SecretID is used to authorize operations against Consul and should be generated from an appropriate cryptographic source.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string SecretID { get; set; }

    /// <summary>
    /// Free form human readable description of the token.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>
    /// The list of policies that should be applied to the token.
    /// A PolicyLink is an object with an "ID" and/or "Name" field to specify a policy.
    /// With the PolicyLink, tokens can be linked to policies either by the policy name or by the policy ID.
    /// When policies are linked by name they will be internally resolved to the policy ID.
    /// With linking tokens internally by IDs, Consul enables policy renaming without breaking tokens.
    /// </summary>
    public ACLPolicyLink[] Policies { get; set; }

    /// <summary>
    /// The list of roles that should be applied to the token.
    /// A RoleLink is an object with an "ID" and/or "Name" field to specify a role.
    /// With the RoleLink, tokens can be linked to roles either by the role name or by the role ID.
    /// When roles are linked by name they will be internally resolved to the role ID. With linking tokens internally by IDs, Consul enables role renaming without breaking tokens. Added in Consul 1.5.0.
    /// </summary>
    public ACLRoleLink[] Roles { get; set; }

    /// <summary>
    /// The list of <see href="https://www.consul.io/docs/acl/acl-system.html#acl-service-identities">service identities</see> that should be applied to the token. Added in Consul 1.5.0.
    /// </summary>
    public ACLServiceIdentity[] ServiceIdentities { get; set; }

    /// <summary>
    /// If true, indicates that the token should not be replicated globally and instead be local to the current datacenter.
    /// </summary>
    public bool Local { get; set; }

    /// <summary>
    /// Specifies the name of the auth method that created this token.
    /// This field is immutable so if present in the body then it must match the existing value.
    /// If not present then the value will be filled in by Consul.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AuthMethod { get; set; }

    /// <summary>
    /// If set this represents the point after which a token should be considered revoked and is eligible for destruction.
    /// The default unset value represents NO expiration. This value must be between 1 minute and 24 hours in the future. Added in Consul 1.5.0.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ExpirationTime { get; set; }

    /// <summary>
    /// This is a convenience field and if set will initialize the ExpirationTime field to a value of CreateTime + ExpirationTTL.
    /// This field is not persisted beyond its initial use. Can be specified in the form of "60s" or "5m" (i.e., 60 seconds or 5 minutes, respectively).
    /// This value must be no smaller than 1 minute and no longer than 24 hours. Added in Consul 1.5.0.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ExpirationTTL { get; set; }
  }

  public class ACLPolicyLink
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ID { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
  }

  public class ACLRoleLink
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ID { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
  }

  public class ACLServiceIdentity
  {
    /// <summary>
    /// The name of the service.
    /// The name must be no longer than 256 characters, must start and end with a lowercase alphanumeric character, and can only contain lowercase alphanumeric characters as well as - and _.
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    /// Specifies the datacenters the effective policy is valid within.
    /// When no datacenters are provided the effective policy is valid in all datacenters including those which do not yet exist but may in the future. 
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string[] Datacenters { get; set; }
  }
}