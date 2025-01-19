using System.ComponentModel.DataAnnotations.Schema;

namespace PallasBot.Domain.Entities;

[Table("github_contributors")]
public record GitHubContributor
{
    [Column("github_login")]
    public string GitHubLogin { get; set; } = string.Empty;

    [Column("is_organization_member")]
    public bool IsOrganizationMember { get; set; }

    [Column("contribute_to")]
    public List<string> ContributeTo { get; set; } = [];

    public void AddContribution(string repo)
    {
        if (ContributeTo.Contains(repo))
        {
            return;
        }

        ContributeTo.Add(repo);
    }

    public void RemoveContribution(string repo)
    {
        ContributeTo.Remove(repo);
    }
}
