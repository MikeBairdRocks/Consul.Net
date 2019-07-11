namespace Consul.Net.Models
{
  /// <summary>
  /// The result of a Consul API write
  /// </summary>
  public class WriteResult : ConsulResult
  {
    public WriteResult() { }
    public WriteResult(WriteResult other) : base(other) { }
  }

  /// <summary>
  /// The result of a Consul API write
  /// </summary>
  /// <typeparam name="T">Must be able to be deserialized from JSON. Some writes return nothing, in which case this should be an empty Object</typeparam>
  public class WriteResult<T> : WriteResult
  {
    /// <summary>
    /// The result of the write
    /// </summary>
    public T Response { get; set; }
    public WriteResult() { }
    public WriteResult(WriteResult other) : base(other) { }
    public WriteResult(WriteResult other, T value) : base(other)
    {
      Response = value;
    }
  }
}