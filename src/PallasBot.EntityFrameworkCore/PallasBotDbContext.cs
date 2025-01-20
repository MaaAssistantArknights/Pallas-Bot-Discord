using Microsoft.EntityFrameworkCore;
using PallasBot.Domain.Entities;

namespace PallasBot.EntityFrameworkCore;

public class PallasBotDbContext : DbContext
{
    public DbSet<DiscordUserRole> DiscordUserRoles => Set<DiscordUserRole>();
    public DbSet<DynamicConfiguration> DynamicConfigurations => Set<DynamicConfiguration>();
    public DbSet<GitHubContributor> GitHubContributors => Set<GitHubContributor>();
    public DbSet<GitHubOrganizationMember> GitHubOrganizationMembers => Set<GitHubOrganizationMember>();
    public DbSet<DiscordUserBinding> DiscordUserBindings => Set<DiscordUserBinding>();

    public PallasBotDbContext(DbContextOptions<PallasBotDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PallasBotDbContext).Assembly);
    }
}
