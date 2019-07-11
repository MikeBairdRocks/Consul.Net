using System;

namespace Consul.Net.Exceptions
{
  /// <summary>
  /// Represents errors that occur during initalization of the Consul client's configuration.
  /// </summary>
  public class ConsulConfigurationException : Exception
  {
    public ConsulConfigurationException() { }
    public ConsulConfigurationException(string message) : base(message) { }
    public ConsulConfigurationException(string message, Exception inner) : base(message, inner) { }
  }
}