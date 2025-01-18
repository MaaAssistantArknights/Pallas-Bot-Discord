namespace PallasBot.Application.Common.Models.Messages;

public record CacheDiscordUserRoleMqo
{
    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public List<ulong> RoleIds { get; set; } = [];
}
