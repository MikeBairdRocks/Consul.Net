using System;
using System.Threading.Tasks;
using Consul.Net.Endpoints.Event;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class EventTest : IDisposable
  {
    AsyncReaderWriterLock.Releaser m_lock;

    public EventTest()
    {
      m_lock = AsyncHelpers.RunSync(() => SelectiveParallel.Parallel());
    }

    public void Dispose()
    {
      m_lock.Dispose();
    }

    [Fact]
    public async Task Event_FireList()
    {
      var client = new ConsulClient();

      var userevent = new UserEvent
      {
        Name = "foo"
      };

      var res = await client.Event.Fire(userevent);

      await Task.Delay(100);

      Assert.NotEqual(TimeSpan.Zero, res.RequestTime);
      Assert.False(string.IsNullOrEmpty(res.Response));

      var events = await client.Event.List();
      Assert.NotEqual(0, events.Response.Length);
      Assert.Equal(res.Response, events.Response[events.Response.Length - 1].ID);
      Assert.Equal(client.Event.IDToIndex(res.Response), events.LastIndex);
    }
  }
}