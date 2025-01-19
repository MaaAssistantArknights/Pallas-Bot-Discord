using Discord.Rest;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Entities;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Common.Consumers;

public record CacheDiscordUserRoleConsumer : IConsumer<CacheDiscordUserRoleMqo>
{
    private readonly PallasBotDbContext _pallasBotDbContext;
    private readonly DiscordRestClient _discordRestClient;

    public CacheDiscordUserRoleConsumer(
        PallasBotDbContext pallasBotDbContext,
        DiscordRestClient discordRestClient)
    {
        _pallasBotDbContext = pallasBotDbContext;
        _discordRestClient = discordRestClient;
    }

    public async Task Consume(ConsumeContext<CacheDiscordUserRoleMqo> context)
    {
        var m = context.Message;
        var now = DateTimeOffset.UtcNow;

        List<ulong> roleIds;
        if (m.ReadFromApi)
        {
            var user = await _discordRestClient.GetGuildUserAsync(m.GuildId, m.UserId);
            if (user is null)
            {
                return;
            }
            roleIds = user.RoleIds.ToList();
        }
        else
        {
            roleIds = m.RoleIds;
        }

        var existing = await _pallasBotDbContext.DiscordUserRoles
            .FirstOrDefaultAsync(x => x.GuildId == m.GuildId && x.UserId == m.UserId);

        if (existing is null)
        {
            await _pallasBotDbContext.DiscordUserRoles.AddAsync(new DiscordUserRole
            {
                GuildId = m.GuildId,
                UserId = m.UserId,
                RoleIds = roleIds,
                UpdateAt = now
            });
        }
        else
        {
            existing.RoleIds = roleIds;
            existing.UpdateAt = now;
            _pallasBotDbContext.DiscordUserRoles.Update(existing);
        }

        await _pallasBotDbContext.SaveChangesAsync();
    }
}
