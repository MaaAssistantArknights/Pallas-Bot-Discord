using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Enums;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Common.Consumers;

public class TryAssignMaaRoleConsumer : IConsumer<TryAssignMaaRoleMqo>
{
    private readonly IDynamicConfigurationService _dynamicConfigurationService;
    private readonly PallasBotDbContext _pallasBotDbContext;
    private readonly ILogger<TryAssignMaaRoleConsumer> _logger;

    public TryAssignMaaRoleConsumer(
        IDynamicConfigurationService dynamicConfigurationService,
        PallasBotDbContext pallasBotDbContext,
        ILogger<TryAssignMaaRoleConsumer> logger)
    {
        _dynamicConfigurationService = dynamicConfigurationService;
        _pallasBotDbContext = pallasBotDbContext;
        _logger = logger;
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

        var githubUser = await _pallasBotDbContext.GitHubUserBindings
            .Where(x => x.GuildId == m.GuildId && x.DiscordUserId == m.UserId)
            .FirstOrDefaultAsync();
        if (githubUser is null)
        {
            return;
        }

        var githubContribution = await _pallasBotDbContext.GitHubContributors
            .Where(x => x.GitHubLogin == githubUser.GitHubLogin)
            .FirstOrDefaultAsync();
        if (githubContribution is null)
        {
            return;
        }

        var cache = await _pallasBotDbContext.DiscordUserRoles
            .FirstOrDefaultAsync(x => x.GuildId == m.GuildId && x.UserId == m.UserId);
        var cacheRoleIds = cache?.RoleIds ?? [];

        var msg = new AssignDiscordRoleMqo
        {
            GuildId = m.GuildId,
            UserId = m.UserId
        };

        if (githubContribution.IsOrganizationMember)
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

        if (githubContribution.ContributeTo.Count > 0)
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
