namespace PallasBot.Application.Common.Models.Messages.GitHub;

public record GitHubLoginResultDmMqo
{
    public required Guid CorrelationId { get; set; }

    public required ulong DiscordUserId { get; set; }

    public required string TextMessage { get; set; }
}
