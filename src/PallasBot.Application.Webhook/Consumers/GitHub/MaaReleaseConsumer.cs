using Discord;
using Discord.Rest;
using MassTransit;
using Microsoft.Extensions.AI;
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
    private readonly AiService _aiService;

    private static readonly ChatMessage SystemPrompt = new(ChatRole.System,
        """
        You are the developer of MaaAssistantArknights, a popular Arknights assistant tool. You are about to release a new version of your tool.
        The changelog is too long to fit in a single message in Discord server. Please summary it, use Markdown format which Discord supports.
        Please output your summary directly without any other words. The summary text should not have more than 12 lines.
        Don't include the title. Because the summary will be fit into an Embed's description.
        It might have some Chinese in the changelog, but the summary should be in English.
        """);

    public MaaReleaseConsumer(
        GitHubApiService gitHubApiService,
        DiscordRestClient discordRestClient,
        IDynamicConfigurationService dynamicConfigurationService,
        ILogger<MaaReleaseConsumer> logger,
        AiService aiService)
    {
        _gitHubApiService = gitHubApiService;
        _discordRestClient = discordRestClient;
        _dynamicConfigurationService = dynamicConfigurationService;
        _logger = logger;
        _aiService = aiService;
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

        // Wait for 3 minutes before checking release detail
        var releaseAt = m.ReleaseAt;
        var shouldCheckAt = releaseAt.AddMinutes(3);
        var now = DateTimeOffset.UtcNow;
        if (shouldCheckAt > now)
        {
            var diff = shouldCheckAt - now;
            _logger.LogInformation("Waiting for {Timespan} to check release detail", diff);
            await Task.Delay(diff);
        }

        // Access token
        var accessToken = await _gitHubApiService.GetGitHubAppAccessTokenAsync();
        var release = await _gitHubApiService.GetReleaseDetailAsync(
            MaaConstants.Organization, MaaConstants.MainRepository, m.ReleaseId, accessToken.Token);

        // Changelog summary
        var summaryMessage = await _aiService.CompleteAsync(
            "ChangelogSummary",
            [SystemPrompt, new ChatMessage(ChatRole.User, release.Body)],
            new ChatOptions
            {
                MaxOutputTokens = 200
            });
        var summary = summaryMessage.Message.Text;
        var modelId = summaryMessage.ModelId ?? "(multiple-models)";

        // Assets buttons
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
        var components = componentBuilder.Build();

        var downloadLinkMessage = assetUrls.Count == 0
            ? string.Empty
            : $"\nOr, download `MAA {release.TagName}` for your platform by clicking the buttons below.";

        // Message building
        var textMessage = $"""
                           ## 🎉 New MAA Release: ** {release.TagName} **

                           Read the full release note [here]({release.HtmlUrl}).

                           Open or reopen your MAA client to get automatic updates.{downloadLinkMessage}

                           """;
        var embed = new EmbedBuilder()
            .WithColor(Color.Purple)
            .WithTitle("Changelog Summary")
            .WithDescription(summary)
            .WithFooter($"This message is generated by AI, it might not be accurate. Model ID: {modelId}")
            .Build();

        foreach (var channelId in channels)
        {
            var channel = (IRestMessageChannel) await _discordRestClient.GetChannelAsync(channelId);

            await channel.SendMessageAsync(
                text: textMessage,
                components: components,
                embed: embed);
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
