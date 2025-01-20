using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using PallasBot.Domain.Entities;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Command.SlashCommands;

[CommandContextType(InteractionContextType.Guild)]
[DefaultMemberPermissions(GuildPermission.Administrator)]
[Group("user", "User operation commands")]
public class UserCommands : InteractionModuleBase
{
    private readonly PallasBotDbContext _dbContext;

    public UserCommands(PallasBotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [SlashCommand("get-info", "Get user info.")]
    public async Task GetInfoAsync(
        [Summary(description: "The user to get info.")] IUser user)
    {
        var guildId = Context.Guild.Id;
        var userId = user.Id;

        var embedBuilder = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("User info")
            .WithImageUrl(user.GetDisplayAvatarUrl());

        embedBuilder.AddField("User ID", userId.ToString(), true);
        embedBuilder.AddField("Username", user.Username, true);

        var cache = await _dbContext.DiscordUserRoles
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);
        if (cache is not null)
        {
            var roles = string.Join(' ', cache.RoleIds.Select(MentionUtils.MentionRole));
            embedBuilder.AddField("Cached Roles", roles, true);
            embedBuilder.AddField("Cached Roles Update", cache.UpdateAt.ToString("u"));
        }

        var binding = _dbContext.DiscordUserBindings
            .FirstOrDefault(x => x.GuildId == guildId && x.DiscordUserId == userId);
        if (binding is not null)
        {
            embedBuilder.AddField("GitHub ID", binding.GitHubUserId, true);
            embedBuilder.AddField("GitHub Login", binding.GitHubLogin, true);

            var contributions = await _dbContext.GitHubContributors
                .Where(x => x.GitHubLogin == binding.GitHubLogin)
                .ToListAsync();
            var isMember = await _dbContext.GitHubOrganizationMembers
                .AnyAsync(x => x.GitHubLogin == binding.GitHubLogin);

            var contributionRepos = string.Join(", ", contributions.Select(x => x.Repository));

            embedBuilder.AddField("Is MAA Member", isMember.ToString(), true);
            embedBuilder.AddField("Contributions", contributionRepos, true);
        }

        var embed = embedBuilder.Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("bind-github", "Force to bind GitHub account.")]
    public async Task BindGitHubAsync(
        [Summary(description: "The user to bind GitHub account.")] IUser user,
        [Summary(description: "GitHub login name")] string loginName,
        [Summary(description: "GitHub user ID")] ulong userId)
    {
        var guildId = Context.Guild.Id;
        var discordUserId = user.Id;

        var existingBinding = await _dbContext.DiscordUserBindings
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.DiscordUserId == discordUserId);
        if (existingBinding is not null)
        {
            existingBinding.GitHubLogin = loginName;
            existingBinding.GitHubUserId = userId;

            _dbContext.Update(existingBinding);
        }
        else
        {
            await _dbContext.AddAsync(new DiscordUserBinding
            {
                GuildId = guildId,
                DiscordUserId = discordUserId,
                GitHubUserId = userId,
                GitHubLogin = loginName,
            });
        }

        await _dbContext.SaveChangesAsync();

        var embed = new EmbedBuilder()
            .WithTitle("GitHub Account Binding (ADMIN OPERATION)")
            .AddField("Discord User ID", discordUserId, true)
            .AddField("Discord User", Context.User.Username, true)
            .AddField("GitHub User ID", userId, true)
            .AddField("GitHub Login", loginName, true)
            .WithColor(Color.Blue)
            .Build();

        await RespondAsync(embed: embed);
    }
}
