using System.Collections.Immutable;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Discord.Net.Rest;

namespace PallasBot.App.Bot.Discord;

public class DiscordHttpRestClient : IRestClient
{
    private readonly string _baseUrl;

    private CancellationToken _cancellationToken;
    private readonly HttpClient _client;

    public DiscordHttpRestClient(string baseUrl, IHttpClientFactory factory)
    {
        _baseUrl = baseUrl;
        _client = factory.CreateClient("DiscordRest");
    }

    public void SetHeader(string key, string? value)
    {
        _client.DefaultRequestHeaders.Remove(key);
        if (value is not null)
        {
            _client.DefaultRequestHeaders.Add(key, value);
        }
    }

    public void SetCancelToken(CancellationToken cancelToken)
    {
        _cancellationToken = cancelToken;
    }

    public async Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken cancelToken, bool headerOnly = false, string? reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders = null)
    {
        var uri = Path.Combine(_baseUrl, endpoint);
        using var restRequest = new HttpRequestMessage(GetMethod(method), uri);
        if (reason is not null)
        {
            restRequest.Headers.Add("X-Audit-Log-Reason", Uri.EscapeDataString(reason));
        }
        if (requestHeaders is not null)
        {
            foreach (var header in requestHeaders)
            {
                restRequest.Headers.Add(header.Key, header.Value);
            }
        }
        return await SendInternalAsync(restRequest, headerOnly, cancelToken).ConfigureAwait(false);
    }

    public async Task<RestResponse> SendAsync(string method, string endpoint, string json, CancellationToken cancelToken, bool headerOnly = false, string? reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders = null)
    {
        var uri = Path.Combine(_baseUrl, endpoint);
        using var restRequest = new HttpRequestMessage(GetMethod(method), uri);
        if (reason is not null)
        {
            restRequest.Headers.Add("X-Audit-Log-Reason", Uri.EscapeDataString(reason));
        }
        if (requestHeaders is not null)
        {
            foreach (var header in requestHeaders)
            {
                restRequest.Headers.Add(header.Key, header.Value);
            }
        }
        restRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return await SendInternalAsync(restRequest, headerOnly, cancelToken).ConfigureAwait(false);
    }

    public Task<RestResponse> SendAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartParams, CancellationToken cancelToken, bool headerOnly = false, string? reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders = null)
    {
        var uri = Path.Combine(_baseUrl, endpoint);
        var restRequest = new HttpRequestMessage(GetMethod(method), uri);
        if (reason is not null)
        {
            restRequest.Headers.Add("X-Audit-Log-Reason", Uri.EscapeDataString(reason));
        }
        if (requestHeaders is not null)
        {
            foreach (var header in requestHeaders)
            {
                restRequest.Headers.Add(header.Key, header.Value);
            }
        }
        var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));

        static StreamContent GetStreamContent(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            return new StreamContent(stream);
        }

        foreach (var p in multipartParams ?? ImmutableDictionary<string, object>.Empty)
        {
            switch (p.Value)
            {
                case string stringValue:
                    { content.Add(new StringContent(stringValue, Encoding.UTF8, "text/plain"), p.Key); continue; }
                case byte[] byteArrayValue:
                    { content.Add(new ByteArrayContent(byteArrayValue), p.Key); continue; }
                case Stream streamValue:
                    { content.Add(GetStreamContent(streamValue), p.Key); continue; }
                case DiscordMultipartFile fileValue:
                    {
                        var streamContent = GetStreamContent(fileValue.Stream);

                        if (fileValue.ContentType is not null)
                        {
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileValue.ContentType);
                        }

                        content.Add(streamContent, p.Key, fileValue.Filename);

                        continue;
                    }
                default:
                    throw new InvalidOperationException($"Unsupported param type \"{p.Value.GetType().Name}\".");
            }
        }

        restRequest.Content = content;
        return SendInternalAsync(restRequest, headerOnly, cancelToken);
    }

    private async Task<RestResponse> SendInternalAsync(HttpRequestMessage request, bool headerOnly, CancellationToken cancellationToken)
    {
        using var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken);
        cancellationToken = cancelTokenSource.Token;
        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var headers = response.Headers.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault(), StringComparer.OrdinalIgnoreCase);
        var stream = (!headerOnly || !response.IsSuccessStatusCode) ? await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false) : null;

        return new RestResponse(response.StatusCode, headers, stream);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private static HttpMethod GetMethod(string method)
    {
        return method switch
        {
            "DELETE" => HttpMethod.Delete,
            "GET" => HttpMethod.Get,
            "PATCH" => HttpMethod.Patch,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            _ => throw new ArgumentOutOfRangeException(nameof(method), $"Unknown HttpMethod: {method}"),
        };
    }
}
