using System;

namespace Consul.Net.Endpoints.KV
{
  /// <summary>
  /// Indicates that the key pair data is invalid
  /// </summary>
  public class InvalidKeyPairException : Exception
  {
    public InvalidKeyPairException()
    {
    }

    public InvalidKeyPairException(string message) : base(message)
    {
    }

    public InvalidKeyPairException(string message, Exception inner) : base(message, inner)
    {
    }
  }
}