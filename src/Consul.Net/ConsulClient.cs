using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Endpoints;
using Consul.Net.Endpoints.ACL;
using Consul.Net.Endpoints.Agent;
using Consul.Net.Endpoints.Catalog;
using Consul.Net.Endpoints.Coordinate;
using Consul.Net.Endpoints.Event;
using Consul.Net.Endpoints.Health;
using Consul.Net.Endpoints.KV;
using Consul.Net.Endpoints.Operator;
using Consul.Net.Endpoints.Query;
using Consul.Net.Endpoints.Session;
using Consul.Net.Endpoints.Snapshot;
using Consul.Net.Endpoints.Status;
using Consul.Net.Models;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Consul.Net.Tests")]
namespace Consul.Net
{
  public interface IConsulClient : IDisposable
  {
    IACLEndpoint ACL { get; }
    Task<IDistributedLock> AcquireLock(LockOptions opts, CancellationToken ct = default(CancellationToken));
    Task<IDistributedLock> AcquireLock(string key, CancellationToken ct = default(CancellationToken));
    Task<IDistributedSemaphore> AcquireSemaphore(SemaphoreOptions opts, CancellationToken ct = default(CancellationToken));
    Task<IDistributedSemaphore> AcquireSemaphore(string prefix, int limit, CancellationToken ct = default(CancellationToken));
    IAgentEndpoint Agent { get; }
    ICatalogEndpoint Catalog { get; }
    IDistributedLock CreateLock(LockOptions opts);
    IDistributedLock CreateLock(string key);
    IEventEndpoint Event { get; }
    Task ExecuteInSemaphore(SemaphoreOptions opts, Action a, CancellationToken ct = default(CancellationToken));
    Task ExecuteInSemaphore(string prefix, int limit, Action a, CancellationToken ct = default(CancellationToken));
    Task ExecuteLocked(LockOptions opts, Action action, CancellationToken ct = default(CancellationToken));

    [Obsolete("This method will be removed in 0.8.0. Replace calls with the method signature ExecuteLocked(LockOptions, Action, CancellationToken)")]
    Task ExecuteLocked(LockOptions opts, CancellationToken ct, Action action);

    Task ExecuteLocked(string key, Action action, CancellationToken ct = default(CancellationToken));

    [Obsolete("This method will be removed in 0.8.0. Replace calls with the method signature ExecuteLocked(string, Action, CancellationToken)")]
    Task ExecuteLocked(string key, CancellationToken ct, Action action);

    IHealthEndpoint Health { get; }
    IKVEndpoint KV { get; }
    IRawEndpoint Raw { get; }
    IDistributedSemaphore Semaphore(SemaphoreOptions opts);
    IDistributedSemaphore Semaphore(string prefix, int limit);
    ISessionEndpoint Session { get; }
    IStatusEndpoint Status { get; }
    IOperatorEndpoint Operator { get; }
    IPreparedQueryEndpoint PreparedQuery { get; }
    ICoordinateEndpoint Coordinate { get; }
    ISnapshotEndpoint Snapshot { get; }
  }

  /// <summary>
  /// Represents a persistant connection to a Consul agent. Instances of this class should be created rarely and reused often.
  /// </summary>
  public class ConsulClient : IConsulClient
  {
    private Lazy<ACLEndpoint> _acl;

    /// <summary>
    /// ACL returns a handle to the ACL endpoints
    /// </summary>
    public IACLEndpoint ACL => _acl.Value;

    private Lazy<CatalogEndpoint> _catalog;

    /// <summary>
    /// Catalog returns a handle to the catalog endpoints
    /// </summary>
    public ICatalogEndpoint Catalog => _catalog.Value;

    private Lazy<HealthEndpoint> _health;

    /// <summary>
    /// Health returns a handle to the health endpoint
    /// </summary>
    public IHealthEndpoint Health => _health.Value;


    private Lazy<CoordinateEndpoint> _coordinate;

    /// <summary>
    /// Session returns a handle to the session endpoints
    /// </summary>
    public ICoordinateEndpoint Coordinate => _coordinate.Value;

    private Lazy<EventEndpoint> _event;

    /// <summary>
    /// Event returns a handle to the event endpoints
    /// </summary>
    public IEventEndpoint Event => _event.Value;

    private Lazy<KVEndpoint> _kv;

    /// <summary>
    /// KV returns a handle to the KV endpoint
    /// </summary>
    public IKVEndpoint KV => _kv.Value;

    private Lazy<OperatorEndpoint> _operator;

    /// <summary>
    /// Operator returns a handle to the operator endpoints.
    /// </summary>
    public IOperatorEndpoint Operator => _operator.Value;

    private Lazy<PreparedQueryEndpoint> _preparedquery;

    /// <summary>
    /// Catalog returns a handle to the catalog endpoints
    /// </summary>
    public IPreparedQueryEndpoint PreparedQuery => _preparedquery.Value;

    private Lazy<SessionEndpoint> _session;

    /// <summary>
    /// Session returns a handle to the session endpoint
    /// </summary>
    public ISessionEndpoint Session => _session.Value;

    private Lazy<SnapshotEndpoint> _snapshot;

    /// <summary>
    /// Catalog returns a handle to the snapshot endpoints
    /// </summary>
    public ISnapshotEndpoint Snapshot => _snapshot.Value;

    private Lazy<StatusEndpoint> _status;

    /// <summary>
    /// Status returns a handle to the status endpoint
    /// </summary>
    public IStatusEndpoint Status => _status.Value;

    private Lazy<RawEndpoint> _raw;

    /// <summary>
    /// Raw returns a handle to query endpoints
    /// </summary>
    public IRawEndpoint Raw => _raw.Value;
    
    private Lazy<AgentEndpoint> _agent;

    /// <summary>
    /// Agent returns a handle to the agent endpoints
    /// </summary>
    public IAgentEndpoint Agent => _agent.Value;
    
    /// <summary>
    /// This class is used to group all the configurable bits of a ConsulClient into a single pointer reference
    /// which is great for implementing reconfiguration later.
    /// </summary>
    private class ConsulClientConfigurationContainer
    {
      internal readonly bool skipClientDispose;
      internal readonly HttpClient HttpClient;
      internal readonly HttpClientHandler HttpHandler;

      public readonly ConsulClientConfiguration Config;

      public ConsulClientConfigurationContainer()
      {
        Config = new ConsulClientConfiguration();
        HttpHandler = new HttpClientHandler();

        HttpClient = new HttpClient(HttpHandler);
        HttpClient.Timeout = TimeSpan.FromMinutes(15);
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");
      }

      #region Old style config handling

      public ConsulClientConfigurationContainer(ConsulClientConfiguration config, HttpClient client)
      {
        skipClientDispose = true;
        Config = config;
        HttpClient = client;
      }

      public ConsulClientConfigurationContainer(ConsulClientConfiguration config)
      {
        Config = config;
        HttpHandler = new HttpClientHandler();
        HttpClient = new HttpClient(HttpHandler);
        HttpClient.Timeout = TimeSpan.FromMinutes(15);
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");
      }

      #endregion

      #region IDisposable Support

      private bool disposedValue = false; // To detect redundant calls

      protected virtual void Dispose(bool disposing)
      {
        if (!disposedValue)
        {
          if (disposing)
          {
            if (HttpClient != null && !skipClientDispose)
            {
              HttpClient.Dispose();
            }

            if (HttpHandler != null)
            {
              HttpHandler.Dispose();
            }
          }

          disposedValue = true;
        }
      }

      //~ConsulClient()
      //{
      //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      //    Dispose(false);
      //}

      // This code added to correctly implement the disposable pattern.
      public void Dispose()
      {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        GC.SuppressFinalize(this);
      }

      public void CheckDisposed()
      {
        if (disposedValue)
        {
          throw new ObjectDisposedException(typeof(ConsulClientConfigurationContainer).FullName.ToString());
        }
      }

      #endregion
    }

    private ConsulClientConfigurationContainer ConfigContainer;

    internal HttpClient HttpClient
    {
      get { return ConfigContainer.HttpClient; }
    }

    internal HttpClientHandler HttpHandler
    {
      get { return ConfigContainer.HttpHandler; }
    }

    public ConsulClientConfiguration Config
    {
      get { return ConfigContainer.Config; }
    }

    internal readonly JsonSerializer serializer = new JsonSerializer();

    #region New style config with Actions

    /// <summary>
    /// Initializes a new Consul client with a default configuration that connects to 127.0.0.1:8500.
    /// </summary>
    public ConsulClient() : this(null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new Consul client with the ability to set portions of the configuration.
    /// </summary>
    /// <param name="configOverride">The Action to modify the default configuration with</param>
    public ConsulClient(Action<ConsulClientConfiguration> configOverride) : this(configOverride, null, null)
    {
    }

    /// <summary>
    /// Initializes a new Consul client with the ability to set portions of the configuration and access the underlying HttpClient for modification.
    /// The HttpClient is modified to set options like the request timeout and headers.
    /// The Timeout property also applies to all long-poll requests and should be set to a value that will encompass all successful requests.
    /// </summary>
    /// <param name="configOverride">The Action to modify the default configuration with</param>
    /// <param name="clientOverride">The Action to modify the HttpClient with</param>
    public ConsulClient(Action<ConsulClientConfiguration> configOverride, Action<HttpClient> clientOverride) : this(configOverride, clientOverride, null)
    {
    }

    /// <summary>
    /// Initializes a new Consul client with the ability to set portions of the configuration and access the underlying HttpClient and WebRequestHandler for modification.
    /// The HttpClient is modified to set options like the request timeout and headers.
    /// The WebRequestHandler is modified to set options like Proxy and Credentials.
    /// The Timeout property also applies to all long-poll requests and should be set to a value that will encompass all successful requests.
    /// </summary>
    /// <param name="configOverride">The Action to modify the default configuration with</param>
    /// <param name="clientOverride">The Action to modify the HttpClient with</param>
    /// <param name="handlerOverride">The Action to modify the WebRequestHandler with</param>
    public ConsulClient(Action<ConsulClientConfiguration> configOverride, Action<HttpClient> clientOverride, Action<HttpClientHandler> handlerOverride)
    {
      var ctr = new ConsulClientConfigurationContainer();

      configOverride?.Invoke(ctr.Config);
      ApplyConfig(ctr.Config, ctr.HttpHandler, ctr.HttpClient);
      handlerOverride?.Invoke(ctr.HttpHandler);
      clientOverride?.Invoke(ctr.HttpClient);

      ConfigContainer = ctr;

      InitializeEndpoints();
    }

    #endregion

    #region Old style config

    /// <summary>
    /// Initializes a new Consul client with the configuration specified.
    /// </summary>
    /// <param name="config">A Consul client configuration</param>
    [Obsolete("This constructor is no longer necessary due to the new Action based constructors and will be removed when 0.8.0 is released." +
              "Please use the ConsulClient(Action<ConsulClientConfiguration>) constructor to set configuration options.", false)]
    public ConsulClient(ConsulClientConfiguration config)
    {
      config.Updated += HandleConfigUpdateEvent;
      var ctr = new ConsulClientConfigurationContainer(config);
      ApplyConfig(ctr.Config, ctr.HttpHandler, ctr.HttpClient);

      ConfigContainer = ctr;
      InitializeEndpoints();
    }

    /// <summary>
    /// Initializes a new Consul client with the configuration specified and a custom HttpClient, which is useful for setting proxies/custom timeouts.
    /// The HttpClient must accept the "application/json" content type and the Timeout property should be set to at least 15 minutes to allow for blocking queries.
    /// </summary>
    /// <param name="config">A Consul client configuration</param>
    /// <param name="client">A custom HttpClient</param>
    [Obsolete("This constructor is no longer necessary due to the new Action based constructors and will be removed when 0.8.0 is released." +
              "Please use one of the ConsulClient(Action<>) constructors instead to set internal options on the HttpClient/WebRequestHandler.", false)]
    public ConsulClient(ConsulClientConfiguration config, HttpClient client)
    {
      var ctr = new ConsulClientConfigurationContainer(config, client);
      if (!ctr.HttpClient.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
      {
        throw new ArgumentException("HttpClient must accept the application/json content type", nameof(client));
      }

      ConfigContainer = ctr;
      InitializeEndpoints();
    }

    #endregion

    private void InitializeEndpoints()
    {
      _acl = new Lazy<ACLEndpoint>(() => new ACLEndpoint(this));
      _agent = new Lazy<AgentEndpoint>(() => new AgentEndpoint(this));
      _catalog = new Lazy<CatalogEndpoint>(() => new CatalogEndpoint(this));
      _coordinate = new Lazy<CoordinateEndpoint>(() => new CoordinateEndpoint(this));
      _event = new Lazy<EventEndpoint>(() => new EventEndpoint(this));
      _health = new Lazy<HealthEndpoint>(() => new HealthEndpoint(this));
      _kv = new Lazy<KVEndpoint>(() => new KVEndpoint(this));
      _operator = new Lazy<OperatorEndpoint>(() => new OperatorEndpoint(this));
      _preparedquery = new Lazy<PreparedQueryEndpoint>(() => new PreparedQueryEndpoint(this));
      _raw = new Lazy<RawEndpoint>(() => new RawEndpoint(this));
      _session = new Lazy<SessionEndpoint>(() => new SessionEndpoint(this));
      _snapshot = new Lazy<SnapshotEndpoint>(() => new SnapshotEndpoint(this));
      _status = new Lazy<StatusEndpoint>(() => new StatusEndpoint(this));
    }

    /// <summary>
    /// Used to created a Semaphore which will operate at the given KV prefix and uses the given limit for the semaphore.
    /// The prefix must have write privileges, and the limit must be agreed upon by all contenders.
    /// </summary>
    /// <param name="prefix">The keyspace prefix (e.g. "locks/semaphore")</param>
    /// <param name="limit">The number of available semaphore slots</param>
    /// <returns>An unlocked semaphore</returns>
    public IDistributedSemaphore Semaphore(string prefix, int limit)
    {
      if (prefix == null)
      {
        throw new ArgumentNullException(nameof(prefix));
      }
      return Semaphore(new SemaphoreOptions(prefix, limit));
    }

    /// <summary>
    /// SemaphoreOpts is used to create a Semaphore with the given options.
    /// The prefix must have write privileges, and the limit must be agreed upon by all contenders.
    /// If a Session is not provided, one will be created.
    /// </summary>
    /// <param name="opts">The semaphore options</param>
    /// <returns>An unlocked semaphore</returns>
    public IDistributedSemaphore Semaphore(SemaphoreOptions opts)
    {
      if (opts == null)
      {
        throw new ArgumentNullException(nameof(opts));
      }
      return new Semaphore(this) { Opts = opts };
    }
    public Task<IDistributedSemaphore> AcquireSemaphore(string prefix, int limit, CancellationToken ct = default(CancellationToken))
    {
      if (string.IsNullOrEmpty(prefix))
      {
        throw new ArgumentNullException(nameof(prefix));
      }
      if (limit <= 0)
      {
        throw new ArgumentNullException(nameof(limit));
      }
      return AcquireSemaphore(new SemaphoreOptions(prefix, limit), ct);
    }
    public async Task<IDistributedSemaphore> AcquireSemaphore(SemaphoreOptions opts, CancellationToken ct = default(CancellationToken))
    {
      if (opts == null)
      {
        throw new ArgumentNullException(nameof(opts));
      }

      var semaphore = Semaphore(opts);
      await semaphore.Acquire(ct).ConfigureAwait(false);
      return semaphore;
    }

    public Task ExecuteInSemaphore(string prefix, int limit, Action a, CancellationToken ct = default(CancellationToken))
    {
      if (string.IsNullOrEmpty(prefix))
      {
        throw new ArgumentNullException(nameof(prefix));
      }
      if (limit <= 0)
      {
        throw new ArgumentNullException(nameof(limit));
      }
      return ExecuteInSemaphore(new SemaphoreOptions(prefix, limit), a, ct);
    }

    public async Task ExecuteInSemaphore(SemaphoreOptions opts, Action a, CancellationToken ct = default(CancellationToken))
    {
      if (opts == null)
      {
        throw new ArgumentNullException(nameof(opts));
      }
      if (a == null)
      {
        throw new ArgumentNullException(nameof(a));
      }

      var semaphore = await AcquireSemaphore(opts, ct).ConfigureAwait(false);

      try
      {
        if (!semaphore.IsHeld)
        {
          throw new LockNotHeldException("Could not obtain the lock");
        }
        a();
      }
      finally
      {
        await semaphore.Release(ct).ConfigureAwait(false);
      }
    }
    
    /// <summary>
    /// CreateLock returns an unlocked lock which can be used to acquire and release the mutex. The key used must have write permissions.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IDistributedLock CreateLock(string key)
    {
      if (string.IsNullOrEmpty(key))
      {
        throw new ArgumentNullException(nameof(key));
      }
      return CreateLock(new LockOptions(key));
    }

    /// <summary>
    /// CreateLock returns an unlocked lock which can be used to acquire and release the mutex. The key used must have write permissions.
    /// </summary>
    /// <param name="opts"></param>
    /// <returns></returns>
    public IDistributedLock CreateLock(LockOptions opts)
    {
      if (opts == null)
      {
        throw new ArgumentNullException(nameof(opts));
      }
      return new Lock(this) { Opts = opts };
    }

    /// <summary>
    /// AcquireLock creates a lock that is already acquired when this call returns.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<IDistributedLock> AcquireLock(string key, CancellationToken ct = default(CancellationToken))
    {
      if (string.IsNullOrEmpty(key))
      {
        throw new ArgumentNullException(nameof(key));
      }
      return AcquireLock(new LockOptions(key), ct);
    }

    /// <summary>
    /// AcquireLock creates a lock that is already acquired when this call returns.
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IDistributedLock> AcquireLock(LockOptions opts, CancellationToken ct = default(CancellationToken))
    {
      if (opts == null)
      {
        throw new ArgumentNullException(nameof(opts));
      }

      var l = CreateLock(opts);
      await l.Acquire(ct).ConfigureAwait(false);
      return l;
    }

    /// <summary>
    /// ExecuteLock accepts a delegate to execute in the context of a lock, releasing the lock when completed.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Task ExecuteLocked(string key, Action action, CancellationToken ct = default(CancellationToken))
    {
      if (string.IsNullOrEmpty(key))
      {
        throw new ArgumentNullException(nameof(key));
      }
      return ExecuteLocked(new LockOptions(key), action, ct);
    }

    /// <summary>
    /// ExecuteLock accepts a delegate to execute in the context of a lock, releasing the lock when completed.
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task ExecuteLocked(LockOptions opts, Action action, CancellationToken ct = default(CancellationToken))
    {
      if (opts == null)
      {
        throw new ArgumentNullException(nameof(opts));
      }
      if (action == null)
      {
        throw new ArgumentNullException(nameof(action));
      }

      var l = await AcquireLock(opts, ct).ConfigureAwait(false);

      try
      {
        if (!l.IsHeld)
        {
          throw new LockNotHeldException("Could not obtain the lock");
        }
        action();
      }
      finally
      {
        await l.Release().ConfigureAwait(false);
      }

    }
    /// <summary>
    /// ExecuteLock accepts a delegate to execute in the context of a lock, releasing the lock when completed.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ct"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Obsolete("This method will be removed in 0.8.0. Replace calls with the method signature ExecuteLocked(string, Action, CancellationToken)")]
    public Task ExecuteLocked(string key, CancellationToken ct, Action action)
    {
      if (string.IsNullOrEmpty(key))
      {
        throw new ArgumentNullException(nameof(key));
      }
      if (action == null)
      {
        throw new ArgumentNullException(nameof(action));
      }
      return ExecuteLocked(new LockOptions(key), action, ct);
    }

    /// <summary>
    /// ExecuteLock accepts a delegate to execute in the context of a lock, releasing the lock when completed.
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="ct"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Obsolete("This method will be removed in 0.8.0. Replace calls with the method signature ExecuteLocked(LockOptions, Action, CancellationToken)")]
    public Task ExecuteLocked(LockOptions opts, CancellationToken ct, Action action)
    {
      if (opts == null)
      {
        throw new ArgumentNullException(nameof(opts));
      }
      return ExecuteLocked(opts, action, ct);
    }
    
    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          Config.Updated -= HandleConfigUpdateEvent;
          if (ConfigContainer != null)
          {
            ConfigContainer.Dispose();
          }
        }

        disposedValue = true;
      }
    }

    //~ConsulClient()
    //{
    //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //    Dispose(false);
    //}

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void CheckDisposed()
    {
      if (disposedValue)
      {
        throw new ObjectDisposedException(typeof(ConsulClient).FullName.ToString());
      }
    }

    #endregion

    void HandleConfigUpdateEvent(object sender, EventArgs e)
    {
      ApplyConfig(sender as ConsulClientConfiguration, HttpHandler, HttpClient);
    }

    void ApplyConfig(ConsulClientConfiguration config, HttpClientHandler handler, HttpClient client)
    {
#pragma warning disable CS0618 // Type or member is obsolete
      if (config.HttpAuth != null)
#pragma warning restore CS0618 // Type or member is obsolete
      {
#pragma warning disable CS0618 // Type or member is obsolete
        handler.Credentials = config.HttpAuth;
#pragma warning restore CS0618 // Type or member is obsolete
      }

      if (config.ClientCertificateSupported)
      {
#pragma warning disable CS0618 // Type or member is obsolete
        if (config.ClientCertificate != null)
#pragma warning restore CS0618 // Type or member is obsolete
        {
          handler.ClientCertificateOptions = ClientCertificateOption.Manual;
#pragma warning disable CS0618 // Type or member is obsolete
          handler.ClientCertificates.Add(config.ClientCertificate);
#pragma warning restore CS0618 // Type or member is obsolete
        }
        else
        {
          handler.ClientCertificateOptions = ClientCertificateOption.Manual;
          handler.ClientCertificates.Clear();
        }
      }

#pragma warning disable CS0618 // Type or member is obsolete
      if (config.DisableServerCertificateValidation)
#pragma warning restore CS0618 // Type or member is obsolete
      {
        handler.ServerCertificateCustomValidationCallback += (certSender, cert, chain, sslPolicyErrors) => { return true; };
      }
      else
      {
        handler.ServerCertificateCustomValidationCallback = null;
      }

      if (!string.IsNullOrEmpty(config.Token))
      {
        if (client.DefaultRequestHeaders.Contains("X-Consul-Token"))
        {
          client.DefaultRequestHeaders.Remove("X-Consul-Token");
        }

        client.DefaultRequestHeaders.Add("X-Consul-Token", config.Token);
      }
    }

    internal GetRequest<TOut> Get<TOut>(string path, QueryOptions opts = null)
    {
      return new GetRequest<TOut>(this, path, opts ?? QueryOptions.Default);
    }

    internal DeleteReturnRequest<TOut> DeleteReturning<TOut>(string path, WriteOptions opts = null)
    {
      return new DeleteReturnRequest<TOut>(this, path, opts ?? WriteOptions.Default);
    }

    internal DeleteRequest Delete(string path, WriteOptions opts = null)
    {
      return new DeleteRequest(this, path, opts ?? WriteOptions.Default);
    }

    internal DeleteAcceptingRequest<TIn> DeleteAccepting<TIn>(string path, TIn body, WriteOptions opts = null)
    {
      return new DeleteAcceptingRequest<TIn>(this, path, body, opts ?? WriteOptions.Default);
    }

    internal PutReturningRequest<TOut> PutReturning<TOut>(string path, WriteOptions opts = null)
    {
      return new PutReturningRequest<TOut>(this, path, opts ?? WriteOptions.Default);
    }

    internal PutRequest<TIn> Put<TIn>(string path, TIn body, WriteOptions opts = null)
    {
      return new PutRequest<TIn>(this, path, body, opts ?? WriteOptions.Default);
    }

    internal PutNothingRequest PutNothing(string path, WriteOptions opts = null)
    {
      return new PutNothingRequest(this, path, opts ?? WriteOptions.Default);
    }

    internal PutRequest<TIn, TOut> Put<TIn, TOut>(string path, TIn body, WriteOptions opts = null)
    {
      return new PutRequest<TIn, TOut>(this, path, body, opts ?? WriteOptions.Default);
    }

    internal PostRequest<TIn> Post<TIn>(string path, TIn body, WriteOptions opts = null)
    {
      return new PostRequest<TIn>(this, path, body, opts ?? WriteOptions.Default);
    }

    internal PostRequest<TIn, TOut> Post<TIn, TOut>(string path, TIn body, WriteOptions opts = null)
    {
      return new PostRequest<TIn, TOut>(this, path, body, opts ?? WriteOptions.Default);
    }
  }
}