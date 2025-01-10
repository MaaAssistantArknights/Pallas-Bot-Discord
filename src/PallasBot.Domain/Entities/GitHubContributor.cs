using System.ComponentModel.DataAnnotations.Schema;

namespace PallasBot.Domain.Entities;

[Table("github_contributors")]
public record GitHubContributor
{
    [Column("github_login")]
    public string GitHubLogin { get; set; } = string.Empty;

    [Column("is_contributor")]
    public bool IsContributor { get; set; }

    [Column("is_team_member")]
    public bool IsTeamMember { get; set; }

    [Column("is_team_leader")]
    public bool IsTeamLeader { get; set; }

    [Column("teams")]
    public string[] Teams { get; set; } = [];
}
