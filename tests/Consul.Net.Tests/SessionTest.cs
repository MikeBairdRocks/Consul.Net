using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Endpoints.Session;
using Consul.Net.Models;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class SessionTest : IDisposable
  {
    AsyncReaderWriterLock.Releaser m_lock;

    public SessionTest()
    {
      m_lock = AsyncHelpers.RunSync(() => SelectiveParallel.Parallel());
    }

    public void Dispose()
    {
      m_lock.Dispose();
    }

    [Fact]
    public async Task Session_CreateDestroy()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create();
      var id = sessionRequest.Response;
      Assert.NotEqual(TimeSpan.Zero, sessionRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(sessionRequest.Response));

      var destroyRequest = await client.Session.Destroy(id);
      Assert.True(destroyRequest.Response);
    }

    [Fact]
    public async Task Session_CreateNoChecksDestroy()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.CreateNoChecks();

      var id = sessionRequest.Response;
      Assert.NotEqual(TimeSpan.Zero, sessionRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(sessionRequest.Response));

      var destroyRequest = await client.Session.Destroy(id);
      Assert.True(destroyRequest.Response);
    }

    [Fact]
    public async Task Session_CreateRenewDestroy()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create(new SessionEntry {TTL = TimeSpan.FromSeconds(10)});

      var id = sessionRequest.Response;
      Assert.NotEqual(TimeSpan.Zero, sessionRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(sessionRequest.Response));

      var renewRequest = await client.Session.Renew(id);
      Assert.NotEqual(TimeSpan.Zero, renewRequest.RequestTime);
      Assert.NotNull(renewRequest.Response.ID);
      Assert.Equal(sessionRequest.Response, renewRequest.Response.ID);
      Assert.True(renewRequest.Response.TTL.HasValue);
      Assert.Equal(renewRequest.Response.TTL.Value.TotalSeconds, TimeSpan.FromSeconds(10).TotalSeconds);

      var destroyRequest = await client.Session.Destroy(id);
      Assert.True(destroyRequest.Response);
    }

    [Fact]
    public async Task Session_CreateRenewDestroyRenew()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create(new SessionEntry {TTL = TimeSpan.FromSeconds(10)});

      var id = sessionRequest.Response;
      Assert.NotEqual(TimeSpan.Zero, sessionRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(sessionRequest.Response));

      var renewRequest = await client.Session.Renew(id);
      Assert.NotEqual(TimeSpan.Zero, renewRequest.RequestTime);
      Assert.NotNull(renewRequest.Response.ID);
      Assert.Equal(sessionRequest.Response, renewRequest.Response.ID);
      Assert.Equal(renewRequest.Response.TTL.Value.TotalSeconds, TimeSpan.FromSeconds(10).TotalSeconds);

      var destroyRequest = await client.Session.Destroy(id);
      Assert.True(destroyRequest.Response);

      try
      {
        renewRequest = await client.Session.Renew(id);
        Assert.True(false, "Session still exists");
      }
      catch (SessionExpiredException ex)
      {
        Assert.IsType<SessionExpiredException>(ex);
      }
    }

    [Fact]
    public async Task Session_Create_RenewPeriodic_Destroy()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create(new SessionEntry {TTL = TimeSpan.FromSeconds(10)});

      var id = sessionRequest.Response;
      Assert.NotEqual(TimeSpan.Zero, sessionRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(sessionRequest.Response));

      var tokenSource = new CancellationTokenSource();
      var ct = tokenSource.Token;

      var renewTask = client.Session.RenewPeriodic(TimeSpan.FromSeconds(1), id, WriteOptions.Default, ct);

      var infoRequest = await client.Session.Info(id);
      Assert.True(infoRequest.LastIndex > 0);
      Assert.True(infoRequest.KnownLeader);

      Assert.Equal(id, infoRequest.Response.ID);

      Assert.True((await client.Session.Destroy(id)).Response);

      try
      {
        renewTask.Wait(10000);
        Assert.True(false, "timedout: missing session did not terminate renewal loop");
      }
      catch (AggregateException ae)
      {
        Assert.IsType<SessionExpiredException>(ae.InnerExceptions[0]);
      }
    }

    [Fact]
    public async Task Session_Create_RenewPeriodic_TTLExpire()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create(new SessionEntry {TTL = TimeSpan.FromSeconds(500)});

      var id = sessionRequest.Response;
      Assert.NotEqual(TimeSpan.Zero, sessionRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(sessionRequest.Response));

      var tokenSource = new CancellationTokenSource();
      var ct = tokenSource.Token;

      try
      {
        var renewTask = client.Session.RenewPeriodic(TimeSpan.FromSeconds(1), id, WriteOptions.Default, ct);
        Assert.True((await client.Session.Destroy(id)).Response);
        renewTask.Wait(10000);
      }
      catch (AggregateException ae)
      {
        foreach (var e in ae.InnerExceptions)
        {
          Assert.IsType<SessionExpiredException>(e);
        }

        return;
      }
      catch (SessionExpiredException ex)
      {
        Assert.IsType<SessionExpiredException>(ex);
        return;
      }

      Assert.True(false, "timed out: missing session did not terminate renewal loop");
    }

    [Fact]
    public async Task Session_Info()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create();

      var id = sessionRequest.Response;

      Assert.NotEqual(TimeSpan.Zero, sessionRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(sessionRequest.Response));

      var infoRequest = await client.Session.Info(id);
      Assert.True(infoRequest.LastIndex > 0);
      Assert.True(infoRequest.KnownLeader);

      Assert.Equal(id, infoRequest.Response.ID);

      Assert.True(string.IsNullOrEmpty(infoRequest.Response.Name));
      Assert.False(string.IsNullOrEmpty(infoRequest.Response.Node));
      Assert.True(infoRequest.Response.CreateIndex > 0);
      Assert.Equal(infoRequest.Response.Behavior, SessionBehavior.Release);

      Assert.True(string.IsNullOrEmpty(infoRequest.Response.Name));
      Assert.True(infoRequest.KnownLeader);

      Assert.True(infoRequest.LastIndex > 0);
      Assert.True(infoRequest.KnownLeader);

      var destroyRequest = await client.Session.Destroy(id);

      Assert.True(destroyRequest.Response);
    }

    [Fact]
    public async Task Session_Node()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create();

      var id = sessionRequest.Response;
      try
      {
        var infoRequest = await client.Session.Info(id);

        var nodeRequest = await client.Session.Node(infoRequest.Response.Node);

        Assert.Contains(sessionRequest.Response, nodeRequest.Response.Select(s => s.ID));
        Assert.NotEqual((ulong) 0, nodeRequest.LastIndex);
        Assert.True(nodeRequest.KnownLeader);
      }
      finally
      {
        var destroyRequest = await client.Session.Destroy(id);

        Assert.True(destroyRequest.Response);
      }
    }

    [Fact]
    public async Task Session_List()
    {
      var client = new ConsulClient();
      var sessionRequest = await client.Session.Create();

      var id = sessionRequest.Response;

      try
      {
        var listRequest = await client.Session.List();

        Assert.Contains(sessionRequest.Response, listRequest.Response.Select(s => s.ID));
        Assert.NotEqual((ulong) 0, listRequest.LastIndex);
        Assert.True(listRequest.KnownLeader);
      }
      finally
      {
        var destroyRequest = await client.Session.Destroy(id);

        Assert.True(destroyRequest.Response);
      }
    }
  }
}