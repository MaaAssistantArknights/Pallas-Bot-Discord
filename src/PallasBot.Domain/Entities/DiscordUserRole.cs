using System.ComponentModel.DataAnnotations.Schema;

namespace PallasBot.Domain.Entities;

[Table("discord_user_role")]
public record DiscordUserRole
{
    [Column("guild_id")]
    public ulong GuildId { get; set; }

    [Column("user_id")]
    public ulong UserId { get; set; }

    [Column("role_ids")]
    public List<ulong> RoleIds { get; set; } = [];

    [Column("update_at")]
    public DateTimeOffset UpdateAt { get; set; }
}
