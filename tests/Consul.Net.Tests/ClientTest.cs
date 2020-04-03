using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Endpoints.KV;
using Consul.Net.Exceptions;
using Consul.Net.Models;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class ClientTest : IDisposable
  {
    AsyncReaderWriterLock.Releaser m_lock;

    public ClientTest()
    {
      m_lock = AsyncHelpers.RunSync(SelectiveParallel.NoParallel);
    }

    public void Dispose()
    {
      m_lock.Dispose();
    }

    [Fact]
    public void Client_DefaultConfig_env()
    {
      const string addr = "1.2.3.4:5678";
      const string token = "abcd1234";
      Environment.SetEnvironmentVariable("CONSUL_HTTP_ADDR", addr);
      Environment.SetEnvironmentVariable("CONSUL_HTTP_TOKEN", token);

      var client = new ConsulClient();
      var config = client.Config;

      Assert.Equal(addr, $"{config.Address.Host}:{config.Address.Port}");
      Assert.Equal(token, config.Token);

      Environment.SetEnvironmentVariable("CONSUL_HTTP_ADDR", string.Empty);
      Environment.SetEnvironmentVariable("CONSUL_HTTP_TOKEN", string.Empty);

      Assert.NotNull(client);
    }

    [Fact]
    public async Task Client_SetQueryOptions()
    {
      var client = new ConsulClient();
      var opts = new QueryOptions
      {
        Datacenter = "foo",
        Consistency = ConsistencyMode.Consistent,
        WaitIndex = 1000,
        WaitTime = new TimeSpan(0, 0, 100),
        Token = "12345"
      };
      var request = client.Get<KVPair>("/v1/kv/foo", opts);

      await Assert.ThrowsAsync<ConsulRequestException>(async () => await request.Execute(CancellationToken.None));

      Assert.Equal("foo", request.Params["dc"]);
      Assert.True(request.Params.ContainsKey("consistent"));
      Assert.Equal("1000", request.Params["index"]);
      Assert.Equal("1m40s", request.Params["wait"]);
    }

    [Fact]
    public async Task Client_SetClientOptions()
    {
      var client = new ConsulClient((c) =>
      {
        c.Datacenter = "foo";
        c.WaitTime = new TimeSpan(0, 0, 100);
        c.Token = "12345";
      });
      var request = client.Get<KVPair>("/v1/kv/foo");

      await Assert.ThrowsAsync<ConsulRequestException>(async () => await request.Execute(CancellationToken.None));

      Assert.Equal("foo", request.Params["dc"]);
      Assert.Equal("1m40s", request.Params["wait"]);
    }

    [Fact]
    public async Task Client_SetWriteOptions()
    {
      var client = new ConsulClient();

      var opts = new WriteOptions
      {
        Datacenter = "foo",
        Token = "12345"
      };

      var request = client.Put("/v1/kv/foo", new KVPair("kv/foo"), opts);

      await Assert.ThrowsAsync<ConsulRequestException>(async () => await request.Execute(CancellationToken.None));

      Assert.Equal("foo", request.Params["dc"]);
    }

    [Fact]
    public async Task Client_CustomHttpClient()
    {
      using (var client = new ConsulClient(x => new ConsulClientConfiguration(), hc =>
      {
        hc.Timeout = TimeSpan.FromDays(10);
        hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      }))
      {
        await client.KV.Put(new KVPair("customhttpclient") {Value = System.Text.Encoding.UTF8.GetBytes("hello world")});
        Assert.Equal(TimeSpan.FromDays(10), client.HttpClient.Timeout);
        Assert.Contains(new MediaTypeWithQualityHeaderValue("application/json"), client.HttpClient.DefaultRequestHeaders.Accept);
        Assert.Equal("hello world", await (await client.HttpClient.GetAsync("http://localhost:8500/v1/kv/customhttpclient?raw")).Content.ReadAsStringAsync());
      }
    }

    [Fact]
    public async Task Client_DisposeBehavior()
    {
      var client = new ConsulClient();
      var opts = new WriteOptions
      {
        Datacenter = "foo",
        Token = "12345"
      };

      client.Dispose();

      var request = client.Put("/v1/kv/foo", new KVPair("kv/foo"), opts);

      await Assert.ThrowsAsync<ObjectDisposedException>(() => request.Execute(CancellationToken.None));
    }

    [Fact]
    public async Task Client_ReuseAndUpdateConfig()
    {
      using (var client = new ConsulClient(c => new ConsulClientConfiguration()))
      {
        await client.KV.Put(new KVPair("kv/reuseconfig") {Flags = 1000});
        Assert.Equal<ulong>(1000, (await client.KV.Get("kv/reuseconfig")).Response.Flags);
      }

      using (var client = new ConsulClient(c => new ConsulClientConfiguration()))
      {
        await client.KV.Put(new KVPair("kv/reuseconfig") {Flags = 2000});
        Assert.Equal<ulong>(2000, (await client.KV.Get("kv/reuseconfig")).Response.Flags);
      }

      using (var client = new ConsulClient(c => new ConsulClientConfiguration()))
      {
        await client.KV.Delete("kv/reuseconfig");
      }
    }

    [Fact]
    public void Client_Constructors()
    {
      Action<ConsulClientConfiguration> cfgAction2 = (cfg) => { cfg.Token = "yep"; };
      Action<ConsulClientConfiguration> cfgAction = (cfg) =>
      {
        cfg.Datacenter = "foo";
        cfgAction2(cfg);
      };

      using (var c = new ConsulClient())
      {
        Assert.NotNull(c.Config);
        Assert.NotNull(c.HttpHandler);
        Assert.NotNull(c.HttpClient);
      }

      using (var c = new ConsulClient(cfgAction))
      {
        Assert.NotNull(c.Config);
        Assert.NotNull(c.HttpHandler);
        Assert.NotNull(c.HttpClient);
        Assert.Equal("foo", c.Config.Datacenter);
      }

      using (var c = new ConsulClient(cfgAction,
        (client) => { client.Timeout = TimeSpan.FromSeconds(30); }))
      {
        Assert.NotNull(c.Config);
        Assert.NotNull(c.HttpHandler);
        Assert.NotNull(c.HttpClient);
        Assert.Equal("foo", c.Config.Datacenter);
        Assert.Equal(TimeSpan.FromSeconds(30), c.HttpClient.Timeout);
      }
    }
  }
}