namespace PallasBot.Application.Common.Models.Messages;

public record AssignDiscordRoleMqo
{
    public required ulong GuildId { get; set; }

    public required ulong UserId { get; set; }

    public List<ulong> ShouldAssignRoleIds { get; set; } = [];

    public List<ulong> ShouldRemoveRoleIds { get; set; } = [];
}
