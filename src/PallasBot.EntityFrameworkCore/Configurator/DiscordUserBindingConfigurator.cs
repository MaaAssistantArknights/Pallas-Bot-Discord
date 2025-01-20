using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PallasBot.Domain.Entities;

namespace PallasBot.EntityFrameworkCore.Configurator;

public class DiscordUserBindingConfigurator : IEntityTypeConfiguration<DiscordUserBinding>
{
    public void Configure(EntityTypeBuilder<DiscordUserBinding> builder)
    {
        builder.HasKey(x => new { x.GuildId, x.DiscordUserId });
    }
}
