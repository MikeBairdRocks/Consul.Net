using System;
using System.Threading.Tasks;
using Consul.Net.Endpoints.ACL;
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

    [SkippableFact]
    public async Task ACL_CreateDelete()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));
      var client = new ConsulClient(c => { c.Token = ConsulRoot; });
      var acl = new ACLToken
      {
        AccessorID = Guid.NewGuid().ToString(),
        Description = "API Test",
        Local = false
      };
      var res = await client.ACL.Create(acl);
      var id = res.Response;

      Assert.NotEqual(TimeSpan.Zero, res.RequestTime);
      Assert.False(string.IsNullOrEmpty(res.Response));
      Assert.Equal(acl.AccessorID, id);
      
      var acl2 = await client.ACL.Get(id);
      Assert.NotNull(acl2.Response);
      Assert.Equal(acl2.Response.Description, acl.Description);

      Assert.True((await client.ACL.Delete(id)).Response);
    }

    [SkippableFact]
    public async Task ACL_CloneUpdateDelete()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));

      var client = new ConsulClient(c => { c.Token = ConsulRoot; });
      var cloneRequest = await client.ACL.Clone(ConsulRoot);
      var aclID = cloneRequest.Response;

      var aclEntry = await client.ACL.Get(aclID);
      const string expectedDescription = "This is a test.";
      aclEntry.Response.Description = expectedDescription;

      var updateToken = new ACLToken
      {
        AccessorID = aclEntry.Response.AccessorID,
        Description = aclEntry.Response.Description,
        Local = false
      };
      await client.ACL.Update(updateToken);

      var aclEntry2 = await client.ACL.Get(aclID);
      Assert.Equal(expectedDescription, aclEntry2.Response.Description);

      var id = cloneRequest.Response;

      Assert.NotEqual(TimeSpan.Zero, cloneRequest.RequestTime);
      Assert.False(string.IsNullOrEmpty(aclID));

      Assert.True((await client.ACL.Delete(id)).Response);
    }

    [SkippableFact]
    public async Task ACL_Get()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));

      var client = new ConsulClient((c) => { c.Token = ConsulRoot; });

      var aclEntry = await client.ACL.Get(ConsulRoot);

      Assert.NotNull(aclEntry.Response);
      Assert.NotEqual(TimeSpan.Zero, aclEntry.RequestTime);
      Assert.Equal(aclEntry.Response.AccessorID, ConsulRoot);
    }

    [SkippableFact]
    public async Task ACL_List()
    {
      Skip.If(string.IsNullOrEmpty(ConsulRoot));

      var client = new ConsulClient((c) => { c.Token = ConsulRoot; });

      var aclList = await client.ACL.List();

      Assert.NotNull(aclList.Response);
      Assert.NotEqual(TimeSpan.Zero, aclList.RequestTime);
      Assert.True(aclList.Response.Length >= 2);
    }
  }
}