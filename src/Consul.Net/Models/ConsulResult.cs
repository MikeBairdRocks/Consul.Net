using System;
using System.Net;

namespace Consul.Net.Models
{
  public abstract class ConsulResult
  {
    /// <summary>
    /// How long the request took
    /// </summary>
    public TimeSpan RequestTime { get; set; }

    /// <summary>
    /// Exposed so that the consumer can to check for a specific status code
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
    public ConsulResult() { }
    public ConsulResult(ConsulResult other)
    {
      RequestTime = other.RequestTime;
      StatusCode = other.StatusCode;
    }
  }
}