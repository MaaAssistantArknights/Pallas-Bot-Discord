using System.Diagnostics;
using System.Text.Json;
using Discord;
using Discord.Rest;
using PallasBot.Application.Common.Abstract;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Application.Webhook.Services;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Enums;

namespace PallasBot.Application.Webhook.Processors;

public class GitHubWebhookProcessor : IWebhookProcessor
{
    private readonly DiscordRestClient _discordRestClient;
    private readonly IDynamicConfigurationService _dynamicConfigurationService;
    private readonly GitHubWebhookValidator _gitHubWebhookValidator;

    public GitHubWebhookProcessor(
        DiscordRestClient discordRestClient,
        IDynamicConfigurationService dynamicConfigurationService,
        GitHubWebhookValidator gitHubWebhookValidator)
    {
        _discordRestClient = discordRestClient;
        _dynamicConfigurationService = dynamicConfigurationService;
        _gitHubWebhookValidator = gitHubWebhookValidator;
    }

    public async Task ProcessAsync(WebhookMessageMqo messageMqo)
    {
        var eventType = messageMqo.GetHeader(WebhookMessageMqo.HeaderEventType);
        var deliveryId = messageMqo.GetHeader(WebhookMessageMqo.HeaderDeliveryId);
        var signature = messageMqo.GetHeader(WebhookMessageMqo.HeaderSignature);

        Activity.Current?.AddTag("webhook.github.event-type", eventType);
        Activity.Current?.AddTag("webhook.github.delivery-id", deliveryId);
        Activity.Current?.AddTag("webhook.github.signature", signature);

        var validationResult = await _gitHubWebhookValidator.ValidateAsync(messageMqo.Body, signature);
        if (validationResult.IsFailed)
        {
            throw new InvalidOperationException($"GitHub webhook validation failed. {string.Join(',', validationResult.Errors.Select(x => x.ToString()))}");
        }

        switch (eventType)
        {
            case "ping":
                return;
            case "release":
                await ProcessReleaseEventAsync(messageMqo.Body);
                return;
            default:
                return;
        }
    }

    private async Task ProcessReleaseEventAsync(string body)
    {
        var channels = (await _dynamicConfigurationService
                .GetAllAsync(DynamicConfigurationKey.MaaReleaseNotificationChannel))
            .Select(x => ulong.TryParse(x.Value, out var v) ? v : (ulong?)null)
            .Where(x => x != null)
            .Select(x => x!.Value)
            .ToList();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var type = root.GetProperty("action").GetString();
        if (type != "published")
        {
            return;
        }

        var release = root.GetProperty("release");

        var name = release.GetProperty("name").GetString()!;
        var htmlUrl = release.GetProperty("html_url").GetString()!;

        var assets = release.GetProperty("assets")
            .EnumerateArray()
            .ToArray();

        var assetUrls = new Dictionary<string, string>();
        var platforms = GetAssetPlatforms(name);
        foreach (var asset in assets)
        {
            var assetName = asset.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(assetName) is false && platforms.TryGetValue(assetName, out var platform))
            {
                var downloadUrl = asset.GetProperty("browser_download_url").GetString();
                var size = asset.GetProperty("size").GetDouble();

                var sizeInMegabytes = size / 1024 / 1024;

                if (string.IsNullOrEmpty(downloadUrl) is false)
                {
                    assetUrls.Add($"{platform} [{sizeInMegabytes:F1} MB]", downloadUrl);
                }
            }
        }

        var componentBuilder = new ComponentBuilder();
        foreach (var (platform, url) in assetUrls.OrderByDescending(x => x.Value))
        {
            componentBuilder.WithButton(
                label: platform,
                style: ButtonStyle.Link,
                url: url);
        }

        var components = componentBuilder.Build();
        var textMessage = $"""
                           ## 🎉 New MAA Release: ** {name} **

                           Read the full release note [here]({htmlUrl}).

                           Open or reopen your MAA client to get automatic updates.
                           Or, download MAA {name} for your platform by clicking the buttons below.

                           """;

        foreach (var channelId in channels)
        {
            var channel = (IRestMessageChannel) await _discordRestClient.GetChannelAsync(channelId);

            await channel.SendMessageAsync(
                text: textMessage,
                components: components);
        }
    }

    private static Dictionary<string, string> GetAssetPlatforms(string name)
    {
        return new Dictionary<string, string>
        {
            [$"MAA-{name}-win-x64.zip"] = "Windows (x64)",
            [$"MAA-{name}-macos-universal.dmg"] = "macOS (Universal, dmg)",
            [$"MAA-{name}-linux-x86_64.tar.gz"] = "Linux (amd64, tar.gz)",
        };
    }
}
