using MassTransit;
using Microsoft.EntityFrameworkCore;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Enums;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Common.Consumers;

public class TryAssignMaaRoleConsumer : IConsumer<TryAssignMaaRoleMqo>
{
    private readonly IDynamicConfigurationService _dynamicConfigurationService;
    private readonly PallasBotDbContext _pallasBotDbContext;

    public TryAssignMaaRoleConsumer(
        IDynamicConfigurationService dynamicConfigurationService,
        PallasBotDbContext pallasBotDbContext)
    {
        _dynamicConfigurationService = dynamicConfigurationService;
        _pallasBotDbContext = pallasBotDbContext;
    }

    public async Task Consume(ConsumeContext<TryAssignMaaRoleMqo> context)
    {
        var m = context.Message;

        var memberRoleId = await _dynamicConfigurationService.GetByGuildAsync(m.GuildId, DynamicConfigurationKey.MaaOrganizationMemberRoleId);
        var contributorRoleId = await _dynamicConfigurationService.GetByGuildAsync(m.GuildId, DynamicConfigurationKey.MaaContributorRoleId);

        if (memberRoleId is null || contributorRoleId is null)
        {
            return;
        }

        var memberRole = ulong.Parse(memberRoleId);
        var contributorRole = ulong.Parse(contributorRoleId);

        var userBinding = await _pallasBotDbContext.DiscordUserBindings
            .FirstOrDefaultAsync(x => x.GuildId == m.GuildId && x.DiscordUserId == m.UserId);
        if (userBinding is null)
        {
            return;
        }

        var cache = await _pallasBotDbContext.DiscordUserRoles
            .FirstOrDefaultAsync(x => x.GuildId == m.GuildId && x.UserId == m.UserId);

        var isMember = await _pallasBotDbContext.GitHubOrganizationMembers
            .AnyAsync(x => x.GitHubLogin == userBinding.GitHubLogin);
        var isContributor = await _pallasBotDbContext.GitHubContributors
            .AnyAsync(x => x.GitHubLogin == userBinding.GitHubLogin);

        var cacheRoleIds = cache?.RoleIds ?? [];

        var msg = new AssignDiscordRoleMqo
        {
            GuildId = m.GuildId,
            UserId = m.UserId
        };

        if (isMember)
        {
            if (cacheRoleIds.Contains(memberRole) is false)
            {
                msg.ShouldAssignRoleIds.Add(memberRole);
            }
        }
        else
        {
            if (cacheRoleIds.Contains(memberRole))
            {
                msg.ShouldRemoveRoleIds.Add(memberRole);
            }
        }

        if (isContributor)
        {
            if (cacheRoleIds.Contains(contributorRole) is false)
            {
                msg.ShouldAssignRoleIds.Add(contributorRole);
            }
        }
        else
        {
            if (cacheRoleIds.Contains(contributorRole))
            {
                msg.ShouldRemoveRoleIds.Add(contributorRole);
            }
        }

        await context.Publish(msg);
    }
}
