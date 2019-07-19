using System;
using System.Reflection;
using Xunit;

namespace Consul.Net.Tests
{
  public class AssemblyTest
  {
    [Fact]
    public void Assembly_IsStrongNamed()
    {
      var type = typeof(ConsulClient);
      var typeInfo = type.GetTypeInfo();
      Assert.Contains("PublicKeyToken", typeInfo.Assembly.FullName);
    }
  }
}