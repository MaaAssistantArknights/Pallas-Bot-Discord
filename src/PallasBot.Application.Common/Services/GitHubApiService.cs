﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PallasBot.Application.Common.Models.GitHub;
using PallasBot.Application.Common.Options;
using PallasBot.Application.Common.Utils;

namespace PallasBot.Application.Common.Services;

public class GitHubApiService
{
    private readonly HttpClient _client;
    private readonly GitHubOptions _options;

    private readonly RsaSecurityKey _rsaSecurityKey;

    // This service is Singleton so it's safe to cache the token here
    private GitHubAppAccessToken? _gitHubAppAccessTokenCache;

    public GitHubApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _client = httpClientFactory.CreateClient("Default");
        _options = GitHubOptions.Get(configuration);

        var pemContent = File.ReadAllText(_options.PemFile);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pemContent);
        _rsaSecurityKey = new RsaSecurityKey(rsa);
    }

    #region Authentication

    public async Task<GitHubDeviceCodeResponse> GetLoginDeviceFlowDeviceCodeAsync()
    {
        using var req = new GitHubHttpRequestBuilder()
            .Post("https://github.com/login/device/code")
            .AcceptJson()
            .WithUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["scope"] = "read:user"
            })
            .Build();

        return await SendRequest<GitHubDeviceCodeResponse>(req);
    }

    public async Task<Result<GitHubDeviceCodeAccessTokenResponse>> GetLoginDeviceFlowAccessTokenAsync(string deviceCode)
    {
        using var req = new GitHubHttpRequestBuilder()
            .Post("https://github.com/login/oauth/access_token")
            .AcceptJson()
            .WithUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["device_code"] = deviceCode,
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
            })
            .Build();

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

    public async Task<GitHubAppAccessToken> GetGitHubAppAccessTokenAsync()
    {
        if (_gitHubAppAccessTokenCache is not null && _gitHubAppAccessTokenCache.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(30))
        {
            return _gitHubAppAccessTokenCache;
        }

        var jwt = GenerateGitHubAppJwt();
        using var req = new GitHubHttpRequestBuilder()
            .Post($"https://api.github.com/app/installations/{_options.InstallationId}/access_tokens")
            .AcceptGitHubJson()
            .WithBearerAuth(jwt)
            .WithLatestApiVersion()
            .Build();

        var result = await SendRequest<GitHubAppAccessToken>(req);
        _gitHubAppAccessTokenCache = result;
        return result;
    }

    #endregion

    #region User

    public async Task<GitHubUser> GetCurrentUserInfoAsync(string accessToken)
    {
        using var req = new GitHubHttpRequestBuilder()
            .Get("https://api.github.com/user")
            .AcceptGitHubJson()
            .WithBearerAuth(accessToken)
            .WithLatestApiVersion()
            .Build();

        return await SendRequest<GitHubUser>(req);
    }

    #endregion

    #region Organization

    public async Task<List<GitHubUser>> GetOrganizationMembersAsync(string org, string accessToken)
    {
        return await GetPaginatedResponseAsync<GitHubUser>(
            () => new GitHubHttpRequestBuilder()
                .WithBearerAuth(accessToken)
                .AcceptGitHubJson()
                .WithLatestApiVersion(),
            $"https://api.github.com/orgs/{org}/members?per_page=100");
    }

    public async Task<List<GitHubUser>> GetRepoContributorsAsync(string org, string repo, string accessToken)
    {
        return await GetPaginatedResponseAsync<GitHubUser>(
            () => new GitHubHttpRequestBuilder()
                .WithBearerAuth(accessToken)
                .AcceptGitHubJson()
                .WithLatestApiVersion(),
            $"https://api.github.com/repos/{org}/{repo}/contributors?per_page=100");
    }

    #endregion

    #region Repository

    public async Task<GitHubRelease> GetReleaseDetailAsync(string organization, string repository, ulong id, string? accessToken = null)
    {
        using var req = new GitHubHttpRequestBuilder()
            .Get($"https://api.github.com/repos/{organization}/{repository}/releases/{id}")
            .WithBearerAuth(accessToken)
            .AcceptGitHubJson()
            .WithLatestApiVersion()
            .Build();

        return await SendRequest<GitHubRelease>(req);
    }

    #endregion

    private async Task<List<T>> GetPaginatedResponseAsync<T>(Func<GitHubHttpRequestBuilder> requestBuilder, string url)
    {
        var nextUrl = url;

        var result = new List<T>();

        while (true)
        {
            using var req = requestBuilder.Invoke().Get(nextUrl).Build();
            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            await using var content = await res.Content.ReadAsStreamAsync();
            var json = await JsonSerializer.DeserializeAsync<List<T>>(content)
                       ?? throw new HttpRequestException("Failed to deserialize response");

            result.AddRange(json);

            var hasLinkHeader = res.Headers.TryGetValues("link", out var linksEnumerable);
            if (hasLinkHeader is false)
            {
                return result;
            }

            var link = (linksEnumerable ?? []).FirstOrDefault();
            if (link is null)
            {
                return result;
            }

            var links = link.Split(',');
            var nextLink = links.FirstOrDefault(l => l.Contains("rel=\"next\""));
            if (nextLink is null)
            {
                return result;
            }

            nextUrl = nextLink.Split(';')[0].Trim().Trim('<', '>');
        }
    }

    private async Task<T> SendRequest<T>(HttpRequestMessage req)
    {
        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();

        await using var content = await res.Content.ReadAsStreamAsync();
        var json = await JsonSerializer.DeserializeAsync<T>(content)
                   ?? throw new HttpRequestException("Failed to deserialize response");

        return json;
    }

    private string GenerateGitHubAppJwt()
    {
        var handler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddMinutes(10),
            Issuer = _options.ClientId,
            SigningCredentials = new SigningCredentials(_rsaSecurityKey, "RS256")
        };

        var token = handler.CreateEncodedJwt(descriptor)
                    ?? throw new InvalidOperationException("Failed to create JWT token");

        return token;
    }
}
