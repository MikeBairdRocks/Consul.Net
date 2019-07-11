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
      Type type = typeof(ConsulClient);
      TypeInfo typeInfo = type.GetTypeInfo();
      string name = typeInfo.Assembly.FullName.ToString();
      Assert.True(typeInfo.Assembly.FullName.Contains("PublicKeyToken"));
    }
  }
}