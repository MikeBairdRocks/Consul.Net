using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Endpoints.Transaction;
using Consul.Net.Exceptions;

namespace Consul.Net.Models
{
  public class PutRequest<TIn> : ConsulRequest
  {
    public WriteOptions Options { get; set; }
    private TIn _body;

    public PutRequest(ConsulClient client, string url, TIn body, WriteOptions options = null) : base(client, url, HttpMethod.Put)
    {
      if (string.IsNullOrEmpty(url))
      {
        throw new ArgumentException(nameof(url));
      }
      _body = body;
      Options = options ?? WriteOptions.Default;
    }

    public async Task<WriteResult> Execute(CancellationToken ct)
    {
      Client.CheckDisposed();
      timer.Start();
      var result = new WriteResult();

      HttpContent content;

      if (typeof(TIn) == typeof(byte[]))
      {
        content = new ByteArrayContent((_body as byte[]) ?? new byte[0]);
      }
      else if (typeof(TIn) == typeof(Stream))
      {
        content = new StreamContent((_body as Stream) ?? new MemoryStream());
      }
      else
      {
        content = new ByteArrayContent(Serialize(_body));
      }

      var message = new HttpRequestMessage(HttpMethod.Put, BuildConsulUri(Endpoint, Params));
      ApplyHeaders(message, Client.Config);
      message.Content = content;

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

  public class PutRequest<TIn, TOut> : ConsulRequest
  {
    public WriteOptions Options { get; set; }
    private TIn _body;

    public PutRequest(ConsulClient client, string url, TIn body, WriteOptions options = null) : base(client, url, HttpMethod.Put)
    {
      if (string.IsNullOrEmpty(url))
      {
        throw new ArgumentException(nameof(url));
      }
      _body = body;
      Options = options ?? WriteOptions.Default;
    }

    public async Task<WriteResult<TOut>> Execute(CancellationToken ct)
    {
      Client.CheckDisposed();
      timer.Start();
      var result = new WriteResult<TOut>();

      HttpContent content = null;

      if (typeof(TIn) == typeof(byte[]))
      {
        var bodyBytes = (_body as byte[]);
        if (bodyBytes != null)
        {
          content = new ByteArrayContent(bodyBytes);
        }
        // If body is null and should be a byte array, then just don't send anything.
      }
      else
      {
        content = new ByteArrayContent(Serialize(_body));
      }

      var message = new HttpRequestMessage(HttpMethod.Put, BuildConsulUri(Endpoint, Params));
      ApplyHeaders(message, Client.Config);
      message.Content = content;
      var response = await Client.HttpClient.SendAsync(message, ct).ConfigureAwait(false);

      result.StatusCode = response.StatusCode;

      ResponseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

      if (!response.IsSuccessStatusCode && (
          (response.StatusCode != HttpStatusCode.NotFound && typeof(TOut) != typeof(TxnResponse)) ||
          (response.StatusCode != HttpStatusCode.Conflict && typeof(TOut) == typeof(TxnResponse))))
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

      if (response.IsSuccessStatusCode ||
          // Special case for KV txn operations
          (response.StatusCode == HttpStatusCode.Conflict && typeof(TOut) == typeof(TxnResponse)))
      {
        result.Response = Deserialize<TOut>(ResponseStream);
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