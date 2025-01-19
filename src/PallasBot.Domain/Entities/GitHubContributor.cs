using System.ComponentModel.DataAnnotations.Schema;

namespace PallasBot.Domain.Entities;

[Table("github_contributors")]
public record GitHubContributor
{
    [Column("github_login")]
    public string GitHubLogin { get; set; } = string.Empty;

    [Column("is_contributor")]
    public bool IsContributor { get; set; }

    [Column("is_organization_member")]
    public bool IsOrganizationMember { get; set; }
}
