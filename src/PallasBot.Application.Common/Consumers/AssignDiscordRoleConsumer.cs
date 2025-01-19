using System.Diagnostics.CodeAnalysis;
using Discord.Rest;
using MassTransit;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Models.Messages;

namespace PallasBot.Application.Common.Consumers;

public class AssignDiscordRoleConsumer : IConsumer<AssignDiscordRoleMqo>
{
    private readonly DiscordRestClient _discordRestClient;
    private readonly ILogger<AssignDiscordRoleConsumer> _logger;

    public AssignDiscordRoleConsumer(DiscordRestClient discordRestClient, ILogger<AssignDiscordRoleConsumer> logger)
    {
        _discordRestClient = discordRestClient;
        _logger = logger;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public async Task Consume(ConsumeContext<AssignDiscordRoleMqo> context)
    {
        var m = context.Message;

        try
        {
            if (m.ShouldAssignRoleIds.Count == 0 && m.ShouldRemoveRoleIds.Count == 0)
            {
                return;
            }

            var user = await _discordRestClient.GetGuildUserAsync(m.GuildId, m.UserId);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found in guild {GuildId}", m.UserId, m.GuildId);
                return;
            }

            var roleIds = user.RoleIds ?? [];

            var assignRole = m.ShouldAssignRoleIds.Except(roleIds).ToArray();
            var removeRole = m.ShouldRemoveRoleIds.Intersect(roleIds).ToArray();

            if (assignRole.Length > 0)
            {
                await user.AddRolesAsync(assignRole);
            }

            if (removeRole.Length > 0)
            {
                await user.RemoveRolesAsync(removeRole);
            }

            await context.Publish(new CacheDiscordUserRoleMqo
            {
                UserId = m.UserId,
                GuildId = m.GuildId,
                ReadFromApi = true,
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error assigning roles to user {UserId} in guild {GuildId}", m.UserId, m.GuildId);
        }
    }
}
