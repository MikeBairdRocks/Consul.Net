using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Query
{
  /// <summary>
  /// QueryTemplate carries the arguments for creating a templated query.
  /// </summary>
  public class QueryTemplate
  {
    /// <summary>
    /// Type specifies the type of the query template. Currently only
    /// "name_prefix_match" is supported. This field is required.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string Type { get; set; }

    /// <summary>
    /// Regexp allows specifying a regex pattern to match against the name
    /// of the query being executed.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Regexp { get; set; }

    public QueryTemplate()
    {
      Type = "name_prefix_match";
    }
  }
}