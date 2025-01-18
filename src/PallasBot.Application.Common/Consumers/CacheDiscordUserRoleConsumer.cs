using MassTransit;
using Microsoft.EntityFrameworkCore;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Entities;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Common.Consumers;

public record CacheDiscordUserRoleConsumer : IConsumer<CacheDiscordUserRoleMqo>
{
    private readonly PallasBotDbContext _pallasBotDbContext;

    public CacheDiscordUserRoleConsumer(PallasBotDbContext pallasBotDbContext)
    {
        _pallasBotDbContext = pallasBotDbContext;
    }

    public async Task Consume(ConsumeContext<CacheDiscordUserRoleMqo> context)
    {
        var m = context.Message;

        var existing = await _pallasBotDbContext.DiscordUserRoles
            .FirstOrDefaultAsync(x => x.GuildId == m.GuildId && x.UserId == m.UserId);

        if (existing is null)
        {
            await _pallasBotDbContext.DiscordUserRoles.AddAsync(new DiscordUserRole
            {
                GuildId = m.GuildId,
                UserId = m.UserId,
                RoleIds = m.RoleIds
            });
        }
        else
        {
            existing.RoleIds = m.RoleIds;
            _pallasBotDbContext.DiscordUserRoles.Update(existing);
        }

        await _pallasBotDbContext.SaveChangesAsync();
    }
}
