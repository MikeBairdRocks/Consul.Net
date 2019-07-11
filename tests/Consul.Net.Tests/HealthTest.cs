using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consul.Net.Endpoints.Agent;
using Consul.Net.Endpoints.Health;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class HealthTest : IDisposable
  {
    AsyncReaderWriterLock.Releaser m_lock;

    public HealthTest()
    {
      m_lock = AsyncHelpers.RunSync(() => SelectiveParallel.Parallel());
    }

    public void Dispose()
    {
      m_lock.Dispose();
    }

    [Fact]
    public async Task Health_Node()
    {
      var client = new ConsulClient();

      var info = await client.Agent.Self();
      var checks = await client.Health.Node((string) info.Response["Config"]["NodeName"]);

      Assert.NotEqual((ulong) 0, checks.LastIndex);
      Assert.NotEqual(0, checks.Response.Length);
    }

    [Fact]
    public async Task Health_Checks()
    {
      var client = new ConsulClient();
      var svcID = KVTest.GenerateTestKeyName();
      var registration = new AgentServiceRegistration
      {
        Name = svcID,
        Tags = new[] {"bar", "baz"},
        Port = 8000,
        Check = new AgentServiceCheck
        {
          TTL = TimeSpan.FromSeconds(15)
        }
      };
      try
      {
        await client.Agent.ServiceRegister(registration);
        var checks = await client.Health.Checks(svcID);
        Assert.NotEqual((ulong) 0, checks.LastIndex);
        Assert.NotEqual(0, checks.Response.Length);
      }
      finally
      {
        await client.Agent.ServiceDeregister(svcID);
      }
    }

    [Fact]
    public async Task Health_Service()
    {
      var client = new ConsulClient();

      var checks = await client.Health.Service("consul", "", false);
      Assert.NotEqual((ulong) 0, checks.LastIndex);
      Assert.NotEqual(0, checks.Response.Length);
    }

    [Fact]
    public async Task Health_State()
    {
      var client = new ConsulClient();

      var checks = await client.Health.State(HealthStatus.Any);
      Assert.NotEqual((ulong) 0, checks.LastIndex);
      Assert.NotEqual(0, checks.Response.Length);
    }

    private struct AggregatedStatusResult
    {
      public string Name;
      public List<HealthCheck> Checks;
      public HealthStatus Expected;
    }

    [Fact]
    public void Health_AggregatedStatus()
    {
      var cases = new List<AggregatedStatusResult>
      {
        new AggregatedStatusResult {Name = "empty", Expected = HealthStatus.Passing, Checks = null},
        new AggregatedStatusResult
        {
          Name = "passing", Expected = HealthStatus.Passing, Checks = new List<HealthCheck>
          {
            new HealthCheck {Status = HealthStatus.Passing}
          }
        },
        new AggregatedStatusResult
        {
          Name = "warning", Expected = HealthStatus.Warning, Checks = new List<HealthCheck>
          {
            new HealthCheck {Status = HealthStatus.Warning}
          }
        },
        new AggregatedStatusResult
        {
          Name = "critical", Expected = HealthStatus.Critical, Checks = new List<HealthCheck>
          {
            new HealthCheck {Status = HealthStatus.Critical}
          }
        },
        new AggregatedStatusResult
        {
          Name = "node_maintenance", Expected = HealthStatus.Maintenance, Checks = new List<HealthCheck>
          {
            new HealthCheck {CheckID = HealthStatus.NodeMaintenance}
          }
        },
        new AggregatedStatusResult
        {
          Name = "service_maintenance", Expected = HealthStatus.Maintenance, Checks = new List<HealthCheck>
          {
            new HealthCheck {CheckID = HealthStatus.ServiceMaintenancePrefix + "service"}
          }
        },
        new AggregatedStatusResult
        {
          Name = "unknown", Expected = HealthStatus.Passing, Checks = new List<HealthCheck>
          {
            new HealthCheck {Status = HealthStatus.Any}
          }
        },
        new AggregatedStatusResult
        {
          Name = "maintenance_over_critical", Expected = HealthStatus.Maintenance, Checks = new List<HealthCheck>
          {
            new HealthCheck {CheckID = HealthStatus.NodeMaintenance},
            new HealthCheck {Status = HealthStatus.Critical}
          }
        },
        new AggregatedStatusResult
        {
          Name = "critical_over_warning", Expected = HealthStatus.Critical, Checks = new List<HealthCheck>
          {
            new HealthCheck {Status = HealthStatus.Critical},
            new HealthCheck {Status = HealthStatus.Warning}
          }
        },
        new AggregatedStatusResult
        {
          Name = "warning_over_passing", Expected = HealthStatus.Warning, Checks = new List<HealthCheck>
          {
            new HealthCheck {Status = HealthStatus.Warning},
            new HealthCheck {Status = HealthStatus.Passing}
          }
        },
        new AggregatedStatusResult
        {
          Name = "lots", Expected = HealthStatus.Warning, Checks = new List<HealthCheck>
          {
            new HealthCheck {Status = HealthStatus.Passing},
            new HealthCheck {Status = HealthStatus.Passing},
            new HealthCheck {Status = HealthStatus.Warning},
            new HealthCheck {Status = HealthStatus.Passing}
          }
        }
      };
      foreach (var test_case in cases)
      {
        Assert.Equal(test_case.Expected, test_case.Checks.AggregatedStatus());
      }
    }
  }
}