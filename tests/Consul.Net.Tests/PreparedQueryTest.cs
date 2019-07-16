using System;
using System.Linq;
using System.Threading.Tasks;
using Consul.Net.Endpoints.Agent;
using Consul.Net.Endpoints.Catalog;
using Consul.Net.Endpoints.Query;
using Consul.Net.Models;
using Consul.Net.Utilities;
using Xunit;

namespace Consul.Net.Tests
{
  public class PreparedQueryTest : IDisposable
  {
    AsyncReaderWriterLock.Releaser m_lock;

    public PreparedQueryTest()
    {
      m_lock = AsyncHelpers.RunSync(() => SelectiveParallel.Parallel());
    }

    public void Dispose()
    {
      m_lock.Dispose();
    }

    [Fact]
    public async Task PreparedQuery_Test()
    {
      var client = new ConsulClient();

      var registration = new CatalogRegistration
      {
        Datacenter = "dc1",
        Node = "foobaz",
        Address = "192.168.10.10",
        Service = new AgentService
        {
          ID = "sql1",
          Service = "sql",
          Tags = new[] {"master", "v1"},
          Port = 8000
        }
      };

      await client.Catalog.Register(registration);

      Assert.NotNull((await client.Catalog.Node("foobaz")).Response);

      var mgmtquerytoken = new QueryOptions {Token = "yep"};

      var sessionRequest = await client.Session.Create();
      var sessionId = sessionRequest.Response;
      
      var definition = new PreparedQueryDefinition {Service = new ServiceQuery {Service = "sql"}, Session = sessionId};

      var id = (await client.PreparedQuery.Create(definition)).Response;
      definition.ID = id;

      var definitions = (await client.PreparedQuery.Get(id)).Response;

      Assert.NotNull(definitions);
      Assert.True(definitions.Length == 1);
      Assert.Equal(definition.Service.Service, definitions[0].Service.Service);

      definitions = (await client.PreparedQuery.List(mgmtquerytoken)).Response;

      Assert.NotNull(definitions);
      Assert.True(definitions.Length == 1);
      Assert.Equal(definition.Service.Service, definitions[0].Service.Service);

      definition.Name = "my-query";

      await client.PreparedQuery.Update(definition);

      definitions = (await client.PreparedQuery.Get(id)).Response;

      Assert.NotNull(definitions);
      Assert.True(definitions.Length == 1);
      Assert.Equal(definition.Name, definitions[0].Name);

      var results = (await client.PreparedQuery.Execute(id)).Response;

      Assert.NotNull(results);
      var nodes = results.Nodes.Where(n => n.Node.Name == "foobaz").ToArray();
      Assert.True(nodes.Length == 1);
      Assert.Equal(nodes[0].Node.Name, "foobaz");

      results = null;
      results = (await client.PreparedQuery.Execute("my-query")).Response;

      Assert.NotNull(results);
      nodes = results.Nodes.Where(n => n.Node.Name == "foobaz").ToArray();
      Assert.True(nodes.Length == 1);
      Assert.Equal(results.Nodes[0].Node.Name, "foobaz");

      await client.PreparedQuery.Delete(id);

      definitions = null;
      definitions = (await client.PreparedQuery.List(mgmtquerytoken)).Response;

      Assert.True(definitions.Length == 0);
    }
  }
}