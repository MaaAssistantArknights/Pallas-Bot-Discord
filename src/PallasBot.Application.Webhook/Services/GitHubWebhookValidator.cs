using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Options;

namespace PallasBot.Application.Webhook.Services;

[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
public class GitHubWebhookValidator : IDisposable
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<GitHubWebhookValidator> _logger;
    private readonly bool _hasKey;
    private readonly HMACSHA256? _algorithm;

    public GitHubWebhookValidator(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        ILogger<GitHubWebhookValidator> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;

        var githubWebhookKey = GitHubOptions.Get(configuration).Webhook.Secret;

        if (string.IsNullOrEmpty(githubWebhookKey))
        {
            _hasKey = false;
            return;
        }

        _hasKey = true;

        var keyBytes = Encoding.UTF8.GetBytes(githubWebhookKey);
        _algorithm = new HMACSHA256(keyBytes);
    }

    public async Task<Result> ValidateAsync(string body, string signature)
    {
        if (_hasKey is false)
        {
            return Result.Ok();
        }

        if (string.IsNullOrEmpty(signature))
        {
            if (_hostEnvironment.IsDevelopment())
            {
                _logger.LogWarning("GitHub signature is missing, return valid because the host environment is Development");
                return Result.Ok();
            }

            return Result.Fail("Missing signature");
        }

        try
        {
            var sig = signature.Split('=', 2)[1];
            var sigBytes = HexToBytes(sig);

            var bodyBytes = Encoding.UTF8.GetBytes(body);
            using var bodyStream = new MemoryStream(bodyBytes);

            var hash = await _algorithm!.ComputeHashAsync(bodyStream);
            var ok = sigBytes.SequenceEqual(hash);

            return ok ? Result.Ok() : Result.Fail("Signature mismatch");
        }
        catch (Exception e)
        {
            return Result.Fail($"Failed to validate signature. {e.GetType().Name}: {e.Message}");
        }
    }

    private static byte[] HexToBytes(string hex)
    {
        var len = hex.Length / 2;
        var bytes = new byte[len];

        for (var i = 0; i < hex.Length; i += 2)
        {
            var c = hex.Substring(i, 2);
            bytes[i / 2] = Convert.ToByte(c, 16);
        }

        return bytes;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _algorithm?.Dispose();
    }
}
