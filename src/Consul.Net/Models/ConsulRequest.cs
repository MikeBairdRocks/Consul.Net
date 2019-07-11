using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Consul.Net.Utilities;
using Newtonsoft.Json;

namespace Consul.Net.Models
{
  public abstract class ConsulRequest
  {
    internal ConsulClient Client { get; set; }
    internal HttpMethod Method { get; set; }
    internal Dictionary<string, string> Params { get; set; }
    internal Stream ResponseStream { get; set; }
    internal string Endpoint { get; set; }

    protected Stopwatch timer = new Stopwatch();

    internal ConsulRequest(ConsulClient client, string url, HttpMethod method)
    {
      Client = client;
      Method = method;
      Endpoint = url;

      Params = new Dictionary<string, string>();
      if (!string.IsNullOrEmpty(client.Config.Datacenter))
      {
        Params["dc"] = client.Config.Datacenter;
      }
      if (client.Config.WaitTime.HasValue)
      {
        Params["wait"] = client.Config.WaitTime.Value.ToGoDuration();
      }
    }

    protected abstract void ApplyOptions(ConsulClientConfiguration clientConfig);
    protected abstract void ApplyHeaders(HttpRequestMessage message, ConsulClientConfiguration clientConfig);

    protected Uri BuildConsulUri(string url, Dictionary<string, string> p)
    {
      var builder = new UriBuilder(Client.Config.Address);
      builder.Path = url;

      ApplyOptions(Client.Config);

      var queryParams = new List<string>(Params.Count / 2);
      foreach (var queryParam in Params)
      {
        if (!string.IsNullOrEmpty(queryParam.Value))
        {
          queryParams.Add(string.Format("{0}={1}", Uri.EscapeDataString(queryParam.Key),
            Uri.EscapeDataString(queryParam.Value)));
        }
        else
        {
          queryParams.Add(string.Format("{0}", Uri.EscapeDataString(queryParam.Key)));
        }
      }

      builder.Query = string.Join("&", queryParams);
      return builder.Uri;
    }

    protected TOut Deserialize<TOut>(Stream stream)
    {
      using (var reader = new StreamReader(stream))
      {
        using (var jsonReader = new JsonTextReader(reader))
        {
          return Client.serializer.Deserialize<TOut>(jsonReader);
        }
      }
    }

    protected byte[] Serialize(object value)
    {
      return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
    }
  }
}