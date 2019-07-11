using System;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class UtilTest
  {
    [Fact]
    public void GoDurationParsing()
    {
      Assert.Equal("150ms", new TimeSpan(0, 0, 0, 0, 150).ToGoDuration());
      Assert.Equal("26h3m4.005s", new TimeSpan(1, 2, 3, 4, 5).ToGoDuration());
      Assert.Equal("2h3m4.005s", new TimeSpan(0, 2, 3, 4, 5).ToGoDuration());
      Assert.Equal("3m4.005s", new TimeSpan(0, 0, 3, 4, 5).ToGoDuration());
      Assert.Equal("4.005s", new TimeSpan(0, 0, 0, 4, 5).ToGoDuration());
      Assert.Equal("5ms", new TimeSpan(0, 0, 0, 0, 5).ToGoDuration());
    }
  }
}