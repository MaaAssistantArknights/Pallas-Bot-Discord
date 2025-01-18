namespace PallasBot.Application.Common.Models.Messages;

public record AssignDiscordRoleMqo
{
    public required ulong GuildId { get; set; }

    public required ulong UserId { get; set; }

    public List<ulong> RoleIds { get; set; } = [];
}
