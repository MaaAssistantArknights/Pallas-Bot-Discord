using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PallasBot.Domain.Entities;

namespace PallasBot.EntityFrameworkCore.Configurator;

public class GitHubOrganizationMemberConfigurator : IEntityTypeConfiguration<GitHubOrganizationMember>
{
    public void Configure(EntityTypeBuilder<GitHubOrganizationMember> builder)
    {
        builder.HasKey(x => x.GitHubLogin);
    }
}
