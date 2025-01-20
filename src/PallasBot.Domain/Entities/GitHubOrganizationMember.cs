using System.ComponentModel.DataAnnotations.Schema;

namespace PallasBot.Domain.Entities;

[Table("github_organization_member")]
public record GitHubOrganizationMember
{
    [Column("github_login")]
    public string GitHubLogin { get; set; } = string.Empty;
}
