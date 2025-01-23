using Discord;
using Discord.Rest;
using MassTransit;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Services;
using PallasBot.Application.Webhook.Models;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Constants;
using PallasBot.Domain.Enums;

namespace PallasBot.Application.Webhook.Consumers.GitHub;

public class MaaReleaseConsumer : IConsumer<MaaReleaseMqo>
{
    private readonly GitHubApiService _gitHubApiService;
    private readonly DiscordRestClient _discordRestClient;
    private readonly IDynamicConfigurationService _dynamicConfigurationService;
    private readonly ILogger<MaaReleaseConsumer> _logger;

    public MaaReleaseConsumer(
        GitHubApiService gitHubApiService,
        DiscordRestClient discordRestClient,
        IDynamicConfigurationService dynamicConfigurationService,
        ILogger<MaaReleaseConsumer> logger)
    {
        _gitHubApiService = gitHubApiService;
        _discordRestClient = discordRestClient;
        _dynamicConfigurationService = dynamicConfigurationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MaaReleaseMqo> context)
    {
        var m = context.Message;

        var channels = (await _dynamicConfigurationService
                .GetAllAsync(DynamicConfigurationKey.MaaReleaseNotificationChannel))
            .Select(x => ulong.TryParse(x.Value, out var v) ? v : (ulong?)null)
            .Where(x => x != null)
            .Select(x => x!.Value)
            .ToList();
        if (channels.Count == 0)
        {
            return;
        }

        var releaseAt = m.ReleaseAt;
        var shouldCheckAt = releaseAt.AddMinutes(3);
        var now = DateTimeOffset.UtcNow;
        if (shouldCheckAt > now)
        {
            var diff = shouldCheckAt - now;
            _logger.LogInformation("Waiting for {Timespan} to check release detail", diff);
            await Task.Delay(diff);
        }

        var accessToken = await _gitHubApiService.GetGitHubAppAccessTokenAsync();
        var release = await _gitHubApiService.GetReleaseDetailAsync(
            MaaConstants.Organization, MaaConstants.MainRepository, m.ReleaseId, accessToken.Token);

        var assetUrls = new Dictionary<string, string>();
        var platforms = GetAssetPlatforms(release.TagName);
        foreach (var asset in release.Assets)
        {
            if (string.IsNullOrEmpty(asset.Name) is false && platforms.TryGetValue(asset.Name, out var platform))
            {
                var sizeInMegabytes = (double)asset.Size / 1024 / 1024;
                assetUrls.Add($"{platform} [{sizeInMegabytes:F1} MB]", asset.BrowserDownloadUrl);
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

        var downloadLinkMessage = assetUrls.Count == 0
            ? string.Empty
            : $"\nOr, download MAA {release.TagName} for your platform by clicking the buttons below.";

        var components = componentBuilder.Build();
        var textMessage = $"""
                           ## 🎉 New MAA Release: ** {release.TagName} **

                           Read the full release note [here]({release.HtmlUrl}).

                           Open or reopen your MAA client to get automatic updates.{downloadLinkMessage}

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
