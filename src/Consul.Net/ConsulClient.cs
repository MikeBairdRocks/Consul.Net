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
    Task<IDistributedLock> AcquireLock(LockOptions opts, CancellationToken ct = default);
    Task<IDistributedLock> AcquireLock(string key, CancellationToken ct = default);
    Task<IDistributedSemaphore> AcquireSemaphore(SemaphoreOptions opts, CancellationToken ct = default);
    Task<IDistributedSemaphore> AcquireSemaphore(string prefix, int limit, CancellationToken ct = default);
    IAgentEndpoint Agent { get; }
    ICatalogEndpoint Catalog { get; }
    IDistributedLock CreateLock(LockOptions opts);
    IDistributedLock CreateLock(string key);
    IEventEndpoint Event { get; }
    Task ExecuteInSemaphore(SemaphoreOptions opts, Action a, CancellationToken ct = default(CancellationToken));
    Task ExecuteInSemaphore(string prefix, int limit, Action a, CancellationToken ct = default(CancellationToken));
    Task ExecuteLocked(LockOptions opts, Action action, CancellationToken ct = default(CancellationToken));
    Task ExecuteLocked(string key, Action action, CancellationToken ct = default(CancellationToken));
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
  /// Represents a persistent connection to a Consul agent. Instances of this class should be created rarely and reused often.
  /// </summary>
  public class ConsulClient : IConsulClient
  {
    private Lazy<ACLEndpoint> _acl;
    private Lazy<CatalogEndpoint> _catalog;
    private Lazy<HealthEndpoint> _health;
    private Lazy<CoordinateEndpoint> _coordinate;
    private Lazy<EventEndpoint> _event;
    private Lazy<KVEndpoint> _kv;
    private Lazy<OperatorEndpoint> _operator;
    private Lazy<PreparedQueryEndpoint> _preparedQuery;
    private Lazy<SessionEndpoint> _session;
    private Lazy<SnapshotEndpoint> _snapshot;
    private Lazy<StatusEndpoint> _status;
    private Lazy<RawEndpoint> _raw;
    private Lazy<AgentEndpoint> _agent;
    
    /// <summary>
    /// ACL returns a handle to the ACL endpoints
    /// </summary>
    public IACLEndpoint ACL => _acl.Value;

    /// <summary>
    /// Catalog returns a handle to the catalog endpoints
    /// </summary>
    public ICatalogEndpoint Catalog => _catalog.Value;

    /// <summary>
    /// Health returns a handle to the health endpoint
    /// </summary>
    public IHealthEndpoint Health => _health.Value;

    /// <summary>
    /// Session returns a handle to the session endpoints
    /// </summary>
    public ICoordinateEndpoint Coordinate => _coordinate.Value;

    /// <summary>
    /// Event returns a handle to the event endpoints
    /// </summary>
    public IEventEndpoint Event => _event.Value;

    /// <summary>
    /// KV returns a handle to the KV endpoint
    /// </summary>
    public IKVEndpoint KV => _kv.Value;

    /// <summary>
    /// Operator returns a handle to the operator endpoints.
    /// </summary>
    public IOperatorEndpoint Operator => _operator.Value;

    /// <summary>
    /// Catalog returns a handle to the catalog endpoints
    /// </summary>
    public IPreparedQueryEndpoint PreparedQuery => _preparedQuery.Value;

    /// <summary>
    /// Session returns a handle to the session endpoint
    /// </summary>
    public ISessionEndpoint Session => _session.Value;

    /// <summary>
    /// Catalog returns a handle to the snapshot endpoints
    /// </summary>
    public ISnapshotEndpoint Snapshot => _snapshot.Value;

    /// <summary>
    /// Status returns a handle to the status endpoint
    /// </summary>
    public IStatusEndpoint Status => _status.Value;

    /// <summary>
    /// Raw returns a handle to query endpoints
    /// </summary>
    public IRawEndpoint Raw => _raw.Value;
    
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
      private bool disposedValue; // To detect redundant calls
      
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

      protected virtual void Dispose(bool disposing)
      {
        if (disposedValue) return;
        if (disposing)
        {
          if (HttpClient != null && !skipClientDispose)
            HttpClient.Dispose();

          HttpHandler?.Dispose();
        }

        disposedValue = true;
      }

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
          throw new ObjectDisposedException(typeof(ConsulClientConfigurationContainer).FullName);
        }
      }
    }

    private ConsulClientConfigurationContainer ConfigContainer;

    internal HttpClient HttpClient => ConfigContainer.HttpClient;

    internal HttpClientHandler HttpHandler => ConfigContainer.HttpHandler;

    public ConsulClientConfiguration Config => ConfigContainer.Config;

    internal readonly JsonSerializer serializer = new JsonSerializer();

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
      _preparedQuery = new Lazy<PreparedQueryEndpoint>(() => new PreparedQueryEndpoint(this));
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

    public Task ExecuteInSemaphore(string prefix, int limit, Action a, CancellationToken ct = default)
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

    public async Task ExecuteInSemaphore(SemaphoreOptions opts, Action a, CancellationToken ct = default)
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
    public Task<IDistributedLock> AcquireLock(string key, CancellationToken ct = default)
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
    public async Task<IDistributedLock> AcquireLock(LockOptions opts, CancellationToken ct = default)
    {
      if (opts == null)
        throw new ArgumentNullException(nameof(opts));

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
    public Task ExecuteLocked(string key, Action action, CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrEmpty(key))
      {
        throw new ArgumentNullException(nameof(key));
      }
      return ExecuteLocked(new LockOptions(key), action, cancellationToken);
    }

    /// <summary>
    /// ExecuteLock accepts a delegate to execute in the context of a lock, releasing the lock when completed.
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task ExecuteLocked(LockOptions opts, Action action, CancellationToken cancellationToken = default)
    {
      if (opts == null)
        throw new ArgumentNullException(nameof(opts));
      
      if (action == null)
        throw new ArgumentNullException(nameof(action));

      var l = await AcquireLock(opts, cancellationToken).ConfigureAwait(false);

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
        throw new ObjectDisposedException(typeof(ConsulClient).FullName);
    }

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