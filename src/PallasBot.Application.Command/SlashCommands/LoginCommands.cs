using Discord;
using Discord.Interactions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PallasBot.Application.Common.Models.Messages.GitHub;
using PallasBot.Application.Common.Services;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Command.SlashCommands;

[CommandContextType(InteractionContextType.Guild)]
[Group("login", "Login commands")]
public class LoginCommands : InteractionModuleBase
{
    private readonly PallasBotDbContext _pallasBotDbContext;
    private readonly GitHubApiService _gitHubApiService;
    private readonly IPublishEndpoint _publishEndpoint;

    public LoginCommands(
        PallasBotDbContext pallasBotDbContext,
        GitHubApiService gitHubApiService,
        IPublishEndpoint publishEndpoint)
    {
        _pallasBotDbContext = pallasBotDbContext;
        _gitHubApiService = gitHubApiService;
        _publishEndpoint = publishEndpoint;
    }

    [SlashCommand("github", "Login and bind your GitHub account")]
    public async Task LoginWithGitHubAsync()
    {
        var existing = await _pallasBotDbContext.DiscordUserBindings
            .FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id &&
                x.DiscordUserId == Context.User.Id &&
                x.GitHubLogin != string.Empty);
        if (existing is not null)
        {
            var errorEmbed = new EmbedBuilder()
                .WithTitle("GitHub Account Binding")
                .WithDescription("You have already bound your GitHub account.")
                .AddField("Username", existing.GitHubLogin)
                .WithColor(Color.Red)
                .Build();
            await RespondAsync(embeds: [errorEmbed], ephemeral: true);
            return;
        }

        var resp = await _gitHubApiService.GetLoginDeviceFlowDeviceCodeAsync();

        var embed = new EmbedBuilder()
            .WithTitle("GitHub Login")
            .WithDescription("Please visit the following URL and enter the code to login.")
            .AddField("URL", resp.VerificationUri)
            .AddField("Code", resp.UserCode)
            .WithColor(Color.DarkGrey)
            .Build();

        var components = new ComponentBuilder()
            .WithButton(label: "Login", style: ButtonStyle.Link, url: resp.VerificationUri)
            .Build();

        await _publishEndpoint.Publish(new GitHubLoginStartMqo
        {
            CorrelationId = Guid.NewGuid(),
            GuildId = Context.Guild.Id,
            DiscordUserId = Context.User.Id,
            DeviceCode = resp.DeviceCode,
            UserCode = resp.UserCode,
            ExpiresIn = resp.ExpiresIn,
            Interval = resp.Interval
        });

        await RespondAsync(embeds: [embed], components: components, ephemeral: true);
    }
}
