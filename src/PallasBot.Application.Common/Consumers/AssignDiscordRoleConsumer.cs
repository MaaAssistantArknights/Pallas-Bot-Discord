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
            if (m.RoleIds.Count == 0)
            {
                return;
            }

            var user = await _discordRestClient.GetGuildUserAsync(m.GuildId, m.UserId);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found in guild {GuildId}", m.UserId, m.GuildId);
                return;
            }

            if (user.RoleIds.All(x => m.RoleIds.Contains(x)))
            {
                return;
            }

            await user.AddRolesAsync(m.RoleIds);

            var userVerify = await _discordRestClient.GetGuildUserAsync(m.GuildId, m.UserId);

            await context.Publish(new CacheDiscordUserRoleMqo
            {
                UserId = m.UserId,
                GuildId = m.GuildId,
                RoleIds = [.. userVerify.RoleIds]
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error assigning roles to user {UserId} in guild {GuildId}", m.UserId, m.GuildId);
        }
    }
}
