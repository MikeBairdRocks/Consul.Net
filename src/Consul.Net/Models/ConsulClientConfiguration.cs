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
    private NetworkCredential _httpauth;
    private bool _disableServerCertificateValidation;
    private X509Certificate2 _clientCertificate;

    internal event EventHandler Updated;

    internal static Lazy<bool> _clientCertSupport = new Lazy<bool>(() => { return Type.GetType("Mono.Runtime") == null; });

    internal bool ClientCertificateSupported { get { return _clientCertSupport.Value; } }

    [Obsolete("Use of DisableServerCertificateValidation should be converted to setting the HttpHandler's ServerCertificateCustomValidationCallback in the ConsulClient constructor" +
              "This property will be removed when 0.8.0 is released.", false)]
    internal bool DisableServerCertificateValidation
    {
      get
      {
        return _disableServerCertificateValidation;
      }
      set
      {
        _disableServerCertificateValidation = value;
        OnUpdated(new EventArgs());
      }
    }

    /// <summary>
    /// The Uri to connect to the Consul agent.
    /// </summary>
    public Uri Address { get; set; }

    /// <summary>
    /// Datacenter to provide with each request. If not provided, the default agent datacenter is used.
    /// </summary>
    public string Datacenter { get; set; }

    /// <summary>
    /// Credentials to use for access to the HTTP API.
    /// This is only needed if an authenticating service exists in front of Consul; Token is used for ACL authentication by Consul.
    /// </summary>
    [Obsolete("Use of HttpAuth should be converted to setting the HttpHandler's Credential property in the ConsulClient constructor" +
              "This property will be removed when 0.8.0 is released.", false)]

    public NetworkCredential HttpAuth
    {
      internal get
      {
        return _httpauth;
      }
      set
      {
        _httpauth = value;
        OnUpdated(new EventArgs());
      }
    }

    /// <summary>
    /// TLS Client Certificate used to secure a connection to a Consul agent. Not supported on Mono.
    /// This is only needed if an authenticating service exists in front of Consul; Token is used for ACL authentication by Consul. This is not the same as RPC Encryption with TLS certificates.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Setting this property will throw a PlatformNotSupportedException on Mono</exception>
    [Obsolete("Use of ClientCertificate should be converted to adding to the HttpHandler's ClientCertificates list in the ConsulClient constructor." +
              "This property will be removed when 0.8.0 is released.", false)]
    public X509Certificate2 ClientCertificate
    {
      internal get
      {
        return _clientCertificate;
      }
      set
      {
        if (!ClientCertificateSupported)
        {
          throw new PlatformNotSupportedException("Client certificates are not supported on this platform");
        }
        _clientCertificate = value;
        OnUpdated(new EventArgs());
      }
    }

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

      var useSsl = (Environment.GetEnvironmentVariable("CONSUL_HTTP_SSL") ?? string.Empty).Trim().ToLowerInvariant();
      if (!string.IsNullOrEmpty(useSsl))
      {
        try
        {
          if (useSsl == "1" || bool.Parse(useSsl))
          {
            consulAddress.Scheme = "https";
          }
        }
        catch (Exception ex)
        {
          throw new ConsulConfigurationException("Could not parse environment variable CONSUL_HTTP_SSL", ex);
        }
      }

      var verifySsl = (Environment.GetEnvironmentVariable("CONSUL_HTTP_SSL_VERIFY") ?? string.Empty).Trim().ToLowerInvariant();
      if (!string.IsNullOrEmpty(verifySsl))
      {
        try
        {
          if (verifySsl == "0" || bool.Parse(verifySsl))
          {
#pragma warning disable CS0618 // Type or member is obsolete
            DisableServerCertificateValidation = true;
#pragma warning restore CS0618 // Type or member is obsolete
          }
        }
        catch (Exception ex)
        {
          throw new ConsulConfigurationException("Could not parse environment variable CONSUL_HTTP_SSL_VERIFY", ex);
        }
      }

      var auth = Environment.GetEnvironmentVariable("CONSUL_HTTP_AUTH");
      if (!string.IsNullOrEmpty(auth))
      {
        var credential = new NetworkCredential();
        if (auth.Contains(":"))
        {
          var split = auth.Split(':');
          credential.UserName = split[0];
          credential.Password = split[1];
        }
        else
        {
          credential.UserName = auth;
        }
#pragma warning disable CS0618 // Type or member is obsolete
        HttpAuth = credential;
#pragma warning restore CS0618 // Type or member is obsolete
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