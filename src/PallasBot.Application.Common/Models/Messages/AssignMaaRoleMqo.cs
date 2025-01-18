namespace PallasBot.Application.Common.Models.Messages;

public record TryAssignMaaRoleMqo
{
    public required ulong GuildId { get; set; }

    public required ulong UserId { get; set; }
}
