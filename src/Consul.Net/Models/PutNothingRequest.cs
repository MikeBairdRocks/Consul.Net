using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Exceptions;

namespace Consul.Net.Models
{
  public class PutNothingRequest : ConsulRequest
  {
    public WriteOptions Options { get; set; }

    public PutNothingRequest(ConsulClient client, string url, WriteOptions options = null) : base(client, url, HttpMethod.Put)
    {
      if (string.IsNullOrEmpty(url))
      {
        throw new ArgumentException(nameof(url));
      }
      Options = options ?? WriteOptions.Default;
    }

    public async Task<WriteResult> Execute(CancellationToken ct)
    {
      Client.CheckDisposed();
      timer.Start();
      var result = new WriteResult();

      var message = new HttpRequestMessage(HttpMethod.Put, BuildConsulUri(Endpoint, Params));
      ApplyHeaders(message, Client.Config);
      var response = await Client.HttpClient.SendAsync(message, ct).ConfigureAwait(false);

      result.StatusCode = response.StatusCode;

      ResponseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

      if (response.StatusCode != HttpStatusCode.NotFound && !response.IsSuccessStatusCode)
      {
        if (ResponseStream == null)
        {
          throw new ConsulRequestException(string.Format("Unexpected response, status code {0}",
            response.StatusCode), response.StatusCode);
        }
        using (var sr = new StreamReader(ResponseStream))
        {
          throw new ConsulRequestException(string.Format("Unexpected response, status code {0}: {1}",
            response.StatusCode, sr.ReadToEnd()), response.StatusCode);
        }
      }

      result.RequestTime = timer.Elapsed;
      timer.Stop();

      return result;
    }

    protected override void ApplyOptions(ConsulClientConfiguration clientConfig)
    {
      if (Options == WriteOptions.Default)
      {
        return;
      }

      if (!string.IsNullOrEmpty(Options.Datacenter))
      {
        Params["dc"] = Options.Datacenter;
      }
    }

    protected override void ApplyHeaders(HttpRequestMessage message, ConsulClientConfiguration clientConfig)
    {
      if (!string.IsNullOrEmpty(Options.Token))
      {
        message.Headers.Add("X-Consul-Token", Options.Token);
      }
    }
  }
}