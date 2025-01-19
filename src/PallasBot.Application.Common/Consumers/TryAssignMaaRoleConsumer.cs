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

        if (memberRoleId is null || contributorRoleId is null || ulong.TryParse(memberRoleId, out _) is false || ulong.TryParse(contributorRoleId, out _) is false)
        {
            return;
        }

        var memberRole = ulong.Parse(memberRoleId);
        var contributorRole = ulong.Parse(contributorRoleId);

        var githubUser = await _pallasBotDbContext.GitHubUserBindings
            .Where(x => x.GuildId == m.GuildId && x.DiscordUserId == m.UserId)
            .Join(
                _pallasBotDbContext.GitHubContributors,
                x => x.GitHubLogin,
                x => x.GitHubLogin,
                (x, y) => new { x.GuildId, x.DiscordUserId, y.GitHubLogin, IsTeamMember = y.IsOrganizationMember, y.IsContributor }
            )
            .FirstOrDefaultAsync();

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (githubUser is null)
        {
            _logger.LogWarning("User {UserId} is not bound to a GitHub account", m.UserId);
            return;
        }

        if (githubUser is { IsTeamMember: false, IsContributor: false })
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

        if (githubUser.IsTeamMember && cacheRoleIds.Contains(memberRole) is false)
        {
            msg.RoleIds.Add(memberRole);
        }
        if (githubUser.IsContributor && cacheRoleIds.Contains(contributorRole) is false)
        {
            msg.RoleIds.Add(memberRole);
        }

        await context.Publish(msg);
    }
}
