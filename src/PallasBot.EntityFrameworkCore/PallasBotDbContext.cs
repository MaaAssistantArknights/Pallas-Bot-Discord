using Microsoft.EntityFrameworkCore;
using PallasBot.Domain.Entities;

namespace PallasBot.EntityFrameworkCore;

public class PallasBotDbContext : DbContext
{
    public DbSet<DynamicConfiguration> DynamicConfigurations => Set<DynamicConfiguration>();
    public DbSet<GitHubUserBinding> GitHubUserBindings => Set<GitHubUserBinding>();

    public PallasBotDbContext(DbContextOptions<PallasBotDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PallasBotDbContext).Assembly);
    }
}
