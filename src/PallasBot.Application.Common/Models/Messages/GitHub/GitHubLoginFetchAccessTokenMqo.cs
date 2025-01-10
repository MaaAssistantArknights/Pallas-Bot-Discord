namespace PallasBot.Application.Common.Models.Messages.GitHub;

public record GitHubLoginFetchAccessTokenMqo
{
    public required Guid CorrelationId { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }

    public required int Interval { get; set; }

    public required string DeviceCode { get; set; } = string.Empty;

    public required string UserCode { get; set; } = string.Empty;
}
