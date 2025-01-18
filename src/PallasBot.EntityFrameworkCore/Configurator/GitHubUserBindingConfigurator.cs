using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PallasBot.Domain.Entities;

namespace PallasBot.EntityFrameworkCore.Configurator;

public class GitHubUserBindingConfigurator : IEntityTypeConfiguration<GitHubUserBinding>
{
    public void Configure(EntityTypeBuilder<GitHubUserBinding> builder)
    {
        builder.HasKey(x => new { x.GuildId, x.DiscordUserId });
    }
}
