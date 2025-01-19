namespace PallasBot.Application.Common.Models.Messages;

public record CacheDiscordUserRoleMqo
{
    public required ulong GuildId { get; set; }

    public required ulong UserId { get; set; }

    public required bool ReadFromApi { get; set; }

    public List<ulong> RoleIds { get; set; } = [];
}
