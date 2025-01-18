using Discord;
using Discord.Rest;
using MassTransit;
using PallasBot.Application.Common.Models.Messages.GitHub;
using PallasBot.Application.Common.Services;
using PallasBot.Domain.Entities;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Command.Consumers;

public class GitHubLoginBindingUserConsumer : IConsumer<GitHubLoginBindingUserMqo>
{
    private readonly DiscordRestClient _discordRestClient;
    private readonly PallasBotDbContext _pallasBotDbContext;
    private readonly GitHubApiService _gitHubApiService;

    public GitHubLoginBindingUserConsumer(
        DiscordRestClient discordRestClient,
        PallasBotDbContext pallasBotDbContext,
        GitHubApiService gitHubApiService)
    {
        _discordRestClient = discordRestClient;
        _pallasBotDbContext = pallasBotDbContext;
        _gitHubApiService = gitHubApiService;
    }

    public async Task Consume(ConsumeContext<GitHubLoginBindingUserMqo> context)
    {
        var m = context.Message;

        var githubUser = await _gitHubApiService.GetCurrentUserInfoAsync(m.AccessToken);

        var binding = new GitHubUserBinding
        {
            GuildId = m.GuildId,
            DiscordUserId = m.DiscordUserId,
            GitHubUserId = githubUser.Id,
            GitHubLogin = githubUser.Login,
        };

        await _pallasBotDbContext.GitHubUserBindings.AddAsync(binding);
        await _pallasBotDbContext.SaveChangesAsync();

        var embed = new EmbedBuilder()
            .WithTitle("GitHub Account Binding")
            .WithDescription("Your GitHub account has been successfully bound to your Discord account.")
            .AddField("GitHub User ID", githubUser.Id, true)
            .AddField("GitHub Login", githubUser.Login, true)
            .AddField("GitHub Email", githubUser.Email, true)
            .WithFooter("We only store your GitHub User ID and GitHub Login. Your GitHub Email is not stored.")
            .WithColor(Color.Green)
            .Build();

        var discordUser = await _discordRestClient.GetUserAsync(m.DiscordUserId);
        await discordUser.SendMessageAsync(embed: embed);

        await context.Publish(new GitHubLoginBindingUserOkMqo
        {
            CorrelationId = m.CorrelationId
        });
    }
}
