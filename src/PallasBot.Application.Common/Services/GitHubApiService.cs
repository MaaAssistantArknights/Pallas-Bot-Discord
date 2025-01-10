using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Configuration;
using PallasBot.Application.Common.Models.GitHub;
using PallasBot.Application.Common.Options;

namespace PallasBot.Application.Common.Services;

public class GitHubApiService
{
    private readonly HttpClient _client;
    private readonly GitHubOptions _options;

    public GitHubApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _client = httpClientFactory.CreateClient("Default");
        _options = GitHubOptions.Get(configuration);
    }

    public async Task<GitHubDeviceCodeResponse> GetLoginDeviceFlowDeviceCodeAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/device/code");
        req.Headers.Add("Accept", "application/json");

        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["scope"] = "read:user"
        });

        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();

        await using var content = await res.Content.ReadAsStreamAsync();
        var json = await JsonSerializer.DeserializeAsync<GitHubDeviceCodeResponse>(content)
                   ?? throw new HttpRequestException("Failed to deserialize response");

        return json;
    }

    public async Task<Result<GitHubDeviceCodeAccessTokenResponse>> GetLoginDeviceFlowAccessTokenAsync(string deviceCode)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
        req.Headers.Add("Accept", "application/json");

        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["device_code"] = deviceCode,
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
        });

        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();

        await using var content = await res.Content.ReadAsStreamAsync();

        using var document = await JsonDocument.ParseAsync(content);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out _))
        {
            return Result.Fail(root.Deserialize<GitHubDeviceCodeFlowErrorResponse>()
                               ?? throw new HttpRequestException("Failed to deserialize response"));
        }

        var json = root.Deserialize<GitHubDeviceCodeAccessTokenResponse>()
            ?? throw new HttpRequestException("Failed to deserialize response");

        return json;
    }

    public async Task<GitHubUser> GetCurrentUserInfoAsync(string accessToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        req.Headers.Add("Accept", "application/vnd.github+json");
        req.Headers.Add("Authorization", $"Bearer {accessToken}");
        req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        var res = await _client.SendAsync(req);

        res.EnsureSuccessStatusCode();

        await using var content = await res.Content.ReadAsStreamAsync();
        var json = await JsonSerializer.DeserializeAsync<GitHubUser>(content)
                   ?? throw new HttpRequestException("Failed to deserialize response");

        return json;
    }
}
