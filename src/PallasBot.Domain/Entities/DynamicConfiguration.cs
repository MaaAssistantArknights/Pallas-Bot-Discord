using System.ComponentModel.DataAnnotations.Schema;
using PallasBot.Domain.Enums;

namespace PallasBot.Domain.Entities;

[Table("dynamic_configuration")]
public record DynamicConfiguration
{
    [Column("guild_id")]
    public ulong GuildId { get; set; }

    [Column("key")]
    public DynamicConfigurationKey Key { get; set; }

    [Column("value")]
    public string Value { get; set; } = string.Empty;

    [Column("update_at")]
    public DateTimeOffset UpdateAt { get; set; }

    [Column("update_by")]
    public ulong UpdateBy { get; set; }
}
