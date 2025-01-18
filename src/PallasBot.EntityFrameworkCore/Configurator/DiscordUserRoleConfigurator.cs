using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PallasBot.Domain.Entities;

namespace PallasBot.EntityFrameworkCore.Configurator;

public class DiscordUserRoleConfigurator : IEntityTypeConfiguration<DiscordUserRole>
{
    public void Configure(EntityTypeBuilder<DiscordUserRole> builder)
    {
        builder.HasKey(x => new { x.GuildId, x.UserId });
    }
}
