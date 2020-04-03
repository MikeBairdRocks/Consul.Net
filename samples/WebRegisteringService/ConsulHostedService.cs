using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net;
using Consul.Net.Endpoints.Agent;
using Consul.Net.Endpoints.Health;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebRegisteringService
{
  public class ConsulHostedService : IHostedService
  {
    private readonly IServer _server;
    private readonly ILogger<ConsulHostedService> _logger;
    private readonly ConsulOptions _options;
    private readonly IConsulClient _consulClient;
    
    private string _registrationId;
    private CancellationTokenSource _cts;

    public ConsulHostedService(IConsulClient consulClient, IServer server, IOptions<ConsulOptions> options, ILogger<ConsulHostedService> logger)
    {
      _server = server;
      _logger = logger;
      _options = options.Value;
      _consulClient = consulClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      // Create a linked token so we can trigger cancellation outside of this token's cancellation
      _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      var features = _server.Features;
      var addresses = features.Get<IServerAddressesFeature>();
      var address = addresses.Addresses.First();
      var uri = new Uri(address);
      var hostName = Dns.GetHostName();
      var hostEntry = await Dns.GetHostEntryAsync(hostName);
      var ip = hostEntry.AddressList[2].ToString(); 
      _registrationId = $"{_options.ServiceId}-{uri.Port}";
      
      var registration = new AgentServiceRegistration
      {
        ID = _registrationId,
        Name = _options.ServiceName,
        Port = uri.Port,
        Check = new AgentServiceCheck
        {
          DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(30),
          Interval = TimeSpan.FromSeconds(30),
          HTTP = $"{uri.Scheme}://{ip}:{uri.Port}/health",
          Status = HealthStatus.Passing

        },

        Tags = new[] {"demo", "test"}
      };

      _logger.LogInformation("Registering in Consul");
      await _consulClient.Agent.ServiceDeregister(_options.ServiceName, _cts.Token);
      await _consulClient.Agent.ServiceRegister(registration, _cts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      _cts.Cancel();
      _logger.LogInformation("Deregistering from Consul");
      try
      {
        await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Deregisteration failed");
      }
    }
  }
}