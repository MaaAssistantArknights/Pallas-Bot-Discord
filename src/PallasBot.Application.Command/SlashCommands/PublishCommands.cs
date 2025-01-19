using Discord;
using Discord.Interactions;
using MassTransit;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Application.Common.Models.Messages.Jobs;
using PallasBot.Domain.Constants;

namespace PallasBot.Application.Command.SlashCommands;

[CommandContextType(InteractionContextType.Guild)]
[DefaultMemberPermissions(GuildPermission.Administrator)]
[Group("publish", "Publish mqo commands")]
public class PublishCommands : InteractionModuleBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PublishCommands(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [SlashCommand("try-assign-maa-role", "Publish TryAssignMaaRoleMqo.")]
    public async Task PublishAssignMaaRoleAsync(
        [Summary(description: "The user that trigger the assign maa role mqo.")] IUser user)
    {
        var mqo = new TryAssignMaaRoleMqo
        {
            GuildId = Context.Guild.Id,
            UserId = user.Id
        };

        await _publishEndpoint.Publish(mqo);

        await RespondAsync($"Message published: {mqo}");
    }

    [SlashCommand("sync-github-organization", "Publish SyncGitHubOrganizationMqo.")]
    public async Task PublishSyncGitHubOrganizationAsync(
        [Summary(description: "Should sync members")] bool syncMembers = false,
        [Summary(description: "Repositories to sync, separated by comma.")] string repositories = "")
    {
        var repos = repositories
            .Split(',')
            .Select(r => r.Trim())
            .Intersect(MaaConstants.Repositories)
            .ToArray();

        var mqo = new SyncGitHubOrganizationMqo
        {
            SyncMembers = syncMembers,
            Repositories = repos
        };

        if (mqo.SyncMembers is false && mqo.Repositories.Length == 0)
        {
            await RespondAsync("This sync job won't do anything. Please provide at least one repository or set sync members to true.");
            return;
        }

        await _publishEndpoint.Publish(mqo);

        await RespondAsync($"Message published: {mqo}");
    }

    [SlashCommand("cache-discord-user-role", "Publish CacheDiscordUserRoleMqo.")]
    public async Task PublishCacheDiscordUserRoleAsync(
        [Summary(description: "The user that trigger the cache discord user role mqo.")] IUser user)
    {
        var mqo = new CacheDiscordUserRoleMqo
        {
            GuildId = Context.Guild.Id,
            UserId = user.Id,
            ReadFromApi = true
        };

        await _publishEndpoint.Publish(mqo);

        await RespondAsync($"Message published: {mqo}");
    }
}
