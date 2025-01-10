namespace PallasBot.Application.Common.Models.Messages.GitHub;

public record GitHubLoginStartMqo
{
    public required Guid CorrelationId { get; set; }

    public required ulong DiscordUserId { get; set; }

    public required string DeviceCode { get; set; }

    public required string UserCode { get; set; }

    public required int ExpiresIn { get; set; }

    public required int Interval { get; set; }
}
