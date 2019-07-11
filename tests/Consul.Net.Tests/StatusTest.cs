using System;
using System.Threading.Tasks;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class StatusTest : IDisposable
  {
    AsyncReaderWriterLock.Releaser m_lock;

    public StatusTest()
    {
      m_lock = AsyncHelpers.RunSync(() => SelectiveParallel.NoParallel());
    }

    public void Dispose()
    {
      m_lock.Dispose();
    }

    [Fact]
    public async Task Status_Leader()
    {
      var client = new ConsulClient();
      var leader = await client.Status.Leader();
      Assert.False(string.IsNullOrEmpty(leader));
    }

    [Fact]
    public async Task Status_Peers()
    {
      var client = new ConsulClient();
      var peers = await client.Status.Peers();
      Assert.True(peers.Length > 0);
    }
  }
}