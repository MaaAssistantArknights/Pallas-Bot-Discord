using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PallasBot.Domain.Entities;

namespace PallasBot.EntityFrameworkCore.Configurator;

public class GitHubContributorConfigurator : IEntityTypeConfiguration<GitHubContributor>
{
    public void Configure(EntityTypeBuilder<GitHubContributor> builder)
    {
        builder.HasKey(x => x.GitHubLogin);
    }
}
