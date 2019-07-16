using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Consul.Net.Exceptions;

namespace Consul.Net.Models
{
  public class PostRequest<TIn> : ConsulRequest
  {
    public WriteOptions Options { get; set; }
    private TIn _body;

    public PostRequest(ConsulClient client, string url, TIn body, WriteOptions options = null) : base(client, url, HttpMethod.Post)
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

      var message = new HttpRequestMessage(HttpMethod.Post, BuildConsulUri(Endpoint, Params));
      ApplyHeaders(message, Client.Config);
      message.Content = content;
      var response = await Client.HttpClient.SendAsync(message, ct).ConfigureAwait(false);

      result.StatusCode = response.StatusCode;

      ResponseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

      if (response.StatusCode != HttpStatusCode.NotFound && !response.IsSuccessStatusCode)
      {
        if (ResponseStream == null)
        {
          throw new ConsulRequestException($"Unexpected response, status code {response.StatusCode}", response.StatusCode);
        }
        using (var sr = new StreamReader(ResponseStream))
        {
          throw new ConsulRequestException($"Unexpected response, status code {response.StatusCode}: {sr.ReadToEnd()}", response.StatusCode);
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

  public class PostRequest<TIn, TOut> : ConsulRequest
  {
    public WriteOptions Options { get; set; }
    private TIn _body;

    public PostRequest(ConsulClient client, string url, TIn body, WriteOptions options = null) : base(client, url, HttpMethod.Post)
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

      var message = new HttpRequestMessage(HttpMethod.Post, BuildConsulUri(Endpoint, Params));
      ApplyHeaders(message, Client.Config);
      message.Content = content;
      var response = await Client.HttpClient.SendAsync(message, ct).ConfigureAwait(false);

      result.StatusCode = response.StatusCode;

      ResponseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

      if (response.StatusCode != HttpStatusCode.NotFound && !response.IsSuccessStatusCode)
      {
        if (ResponseStream == null)
        {
          throw new ConsulRequestException($"Unexpected response, status code {response.StatusCode}", response.StatusCode);
        }
        using (var sr = new StreamReader(ResponseStream))
        {
          throw new ConsulRequestException($"Unexpected response, status code {response.StatusCode}: {sr.ReadToEnd()}", response.StatusCode);
        }
      }

      if (response.IsSuccessStatusCode)
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