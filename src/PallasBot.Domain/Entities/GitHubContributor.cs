﻿using System.ComponentModel.DataAnnotations.Schema;

namespace PallasBot.Domain.Entities;

[Table("github_contributors")]
public record GitHubContributor
{
    [Column("github_login")]
    public string GitHubLogin { get; set; } = string.Empty;

    [Column("repository")]
    public string Repository { get; set; } = string.Empty;
}
