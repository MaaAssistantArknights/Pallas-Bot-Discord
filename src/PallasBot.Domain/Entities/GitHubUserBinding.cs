﻿using System.ComponentModel.DataAnnotations.Schema;

namespace PallasBot.Domain.Entities;

[Table("github_user_binding")]
public record GitHubUserBinding
{
    [Column("discord_user_id")]
    public ulong DiscordUserId { get; set; }

    [Column("github_user_id")]
    public ulong GitHubUserId { get; set; }

    [Column("github_login")]
    public string GitHubLogin { get; set; } = string.Empty;
}
