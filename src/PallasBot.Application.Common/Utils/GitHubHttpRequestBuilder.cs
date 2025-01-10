using System.Text;
using System.Text.Json;

namespace PallasBot.Application.Common.Utils;

public class GitHubHttpRequestBuilder : IDisposable
{
    private readonly HttpRequestMessage _httpRequestMessage;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public GitHubHttpRequestBuilder()
    {
        _httpRequestMessage = new HttpRequestMessage();
    }

    public GitHubHttpRequestBuilder Get(string uri)
    {
        _httpRequestMessage.Method = HttpMethod.Get;
        _httpRequestMessage.RequestUri = new Uri(uri);
        return this;
    }

    public GitHubHttpRequestBuilder Post(string uri)
    {
        _httpRequestMessage.Method = HttpMethod.Post;
        _httpRequestMessage.RequestUri = new Uri(uri);
        return this;
    }

    public GitHubHttpRequestBuilder WithBearerAuth(string token)
    {
        _httpRequestMessage.Headers.Add("Authorization", $"Bearer {token}");
        return this;
    }

    public GitHubHttpRequestBuilder AcceptJson()
    {
        _httpRequestMessage.Headers.Add("Accept", "application/json");
        return this;
    }

    public GitHubHttpRequestBuilder AcceptGitHubJson()
    {
        _httpRequestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
        return this;
    }

    public GitHubHttpRequestBuilder WithLatestApiVersion()
    {
        _httpRequestMessage.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        return this;
    }

    public GitHubHttpRequestBuilder WithUrlEncodedContent(Dictionary<string, string> content)
    {
        _httpRequestMessage.Content = new FormUrlEncodedContent(content);
        return this;
    }

    public GitHubHttpRequestBuilder WithJsonContent<T>(T content)
    {
        var json = JsonSerializer.Serialize(content);
        _httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return this;
    }

    public HttpRequestMessage Build()
    {
        return _httpRequestMessage;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _httpRequestMessage.Dispose();
    }
}
