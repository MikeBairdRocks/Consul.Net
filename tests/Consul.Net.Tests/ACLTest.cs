using System;
using System.Threading.Tasks;
using Consul.Net.Endpoints.ACL;
using Consul.Net.Models;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class ACLTest : IDisposable
  {
    AsyncReaderWriterLock.Releaser m_lock;

    public ACLTest()
    {
      m_lock = AsyncHelpers.RunSync(() => SelectiveParallel.Parallel());
    }

    public void Dispose()
    {
      m_lock.Dispose();
    }

    internal const string ConsulRoot = "eba37d50-2fd8-42f2-b9f6-9c7c7a55890e";
    internal const string Description = "API Test";

    [SkippableFact]
    public async Task ACL_Delete()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));
     
      // Arrange
      var client = new ConsulClient(c => { c.Token = ConsulRoot; });
      var acl = await Create(client);
      var id = acl.Response;
     
      // Act
      var result = await client.ACL.Get(id);

      // Assert
      Assert.NotNull(result.Response);
      Assert.Equal(result.Response.Description, Description);
      Assert.True((await client.ACL.Delete(id)).Response);
    }

    [SkippableFact]
    public async Task ACL_Update()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));

      // Arrange
      const string expectedDescription = "This is a test.";
      var client = new ConsulClient(c => { c.Token = ConsulRoot; });
      var acl = await Create(client);
      var id = acl.Response;
     
      // Act
      var updateToken = new ACLToken
      {
        AccessorID = id,
        Description = expectedDescription,
        Local = false
      };
      var updateResult = await client.ACL.Update(updateToken);
      var result = await client.ACL.Get(id);
      
      // Assert
      Assert.Equal(expectedDescription, result.Response.Description);
      Assert.NotEqual(TimeSpan.Zero, updateResult.RequestTime);
      Assert.NotEqual(TimeSpan.Zero, result.RequestTime);
      Assert.False(string.IsNullOrEmpty(id));

      Assert.True((await client.ACL.Delete(id)).Response);
    }
    
    [SkippableFact]
    public async Task ACL_Clone()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));

      // Arrange
      var client = new ConsulClient(c => { c.Token = ConsulRoot; });
      var acl = await Create(client);
      var id = acl.Response;
     
      // Act
      var cloneResult = await client.ACL.Clone(id);
      var cloneId = cloneResult.Response;
      var result = await client.ACL.Get(cloneResult.Response);
      
      // Assert
      Assert.Equal(Description, result.Response.Description);
      Assert.NotEqual(TimeSpan.Zero, cloneResult.RequestTime);
      Assert.NotEqual(id, cloneId);
      Assert.False(string.IsNullOrEmpty(cloneId));

      // Clean up
      Assert.True((await client.ACL.Delete(id)).Response);
      Assert.True((await client.ACL.Delete(cloneId)).Response);
    }

    [SkippableFact]
    public async Task ACL_Get()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));

      // Arrange
      var client = new ConsulClient(c => { c.Token = ConsulRoot; });
      var acl = await Create(client);
      var id = acl.Response;
      
      // Act
      var result = await client.ACL.Get(id);

      // Assert
      Assert.NotNull(result.Response);
      Assert.Equal(Description, result.Response.Description);
      Assert.NotEqual(TimeSpan.Zero, result.RequestTime);
    }

    [SkippableFact]
    public async Task ACL_List()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));

      // Arrange
      var client = new ConsulClient(c => { c.Token = ConsulRoot; });

      // Act
      var result = await client.ACL.List();

      // Assert
      Assert.NotNull(result.Response);
      Assert.NotEqual(TimeSpan.Zero, result.RequestTime);
      Assert.True(result.Response.Length >= 2);
    }
    
    private async Task<WriteResult<string>> Create(IConsulClient client)
    {
      var acl = new ACLToken
      {
        AccessorID = Guid.NewGuid().ToString(),
        Description = Description,
        Local = false
      };
      var result = await client.ACL.Create(acl);

      Assert.NotEqual(TimeSpan.Zero, result.RequestTime);
      Assert.Equal(acl.AccessorID, result.Response);
      Assert.False(string.IsNullOrEmpty(result.Response));
      
      return result;
    }
  }
}