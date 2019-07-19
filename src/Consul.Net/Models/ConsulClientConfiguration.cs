using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Consul.Net.Exceptions;

namespace Consul.Net.Models
{
  /// <summary>
  /// Represents the configuration options for a Consul client.
  /// </summary>
  public class ConsulClientConfiguration
  {
    internal event EventHandler Updated;

    /// <summary>
    /// The Uri to connect to the Consul agent.
    /// </summary>
    public Uri Address { get; set; }

    /// <summary>
    /// Datacenter to provide with each request. If not provided, the default agent datacenter is used.
    /// </summary>
    public string Datacenter { get; set; }

    /// <summary>
    /// Token is used to provide an ACL token which overrides the agent's default token. This ACL token is used for every request by
    /// clients created using this configuration.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// WaitTime limits how long a Watch will block. If not provided, the agent default values will be used.
    /// </summary>
    public TimeSpan? WaitTime { get; set; }

    /// <summary>
    /// Creates a new instance of a Consul client configuration.
    /// </summary>
    /// <exception cref="ConsulConfigurationException">An error that occured while building the configuration.</exception>
    public ConsulClientConfiguration()
    {
      var consulAddress = new UriBuilder("http://127.0.0.1:8500");
      ConfigureFromEnvironment(consulAddress);
      Address = consulAddress.Uri;
    }

    /// <summary>
    /// Builds configuration based on environment variables.
    /// </summary>
    /// <exception cref="ConsulConfigurationException">An environment variable could not be parsed</exception>
    private void ConfigureFromEnvironment(UriBuilder consulAddress)
    {
      var envAddr = (Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ?? string.Empty).Trim().ToLowerInvariant();
      if (!string.IsNullOrEmpty(envAddr))
      {
        var addrParts = envAddr.Split(':');
        for (var i = 0; i < addrParts.Length; i++)
        {
          addrParts[i] = addrParts[i].Trim();
        }
        if (!string.IsNullOrEmpty(addrParts[0]))
        {
          consulAddress.Host = addrParts[0];
        }
        if (addrParts.Length > 1 && !string.IsNullOrEmpty(addrParts[1]))
        {
          try
          {
            consulAddress.Port = ushort.Parse(addrParts[1]);
          }
          catch (Exception ex)
          {
            throw new ConsulConfigurationException("Failed parsing port from environment variable CONSUL_HTTP_ADDR", ex);
          }
        }
      }

      if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN")))
      {
        Token = Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN");
      }
    }

    internal virtual void OnUpdated(EventArgs e)
    {
      // Make a temporary copy of the event to avoid possibility of
      // a race condition if the last subscriber unsubscribes
      // immediately after the null check and before the event is raised.
      var handler = Updated;

      // Event will be null if there are no subscribers
      handler?.Invoke(this, e);
    }
  }
}