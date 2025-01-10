namespace PallasBot.Application.Common.Models.Messages.GitHub;

public record GitHubLoginFetchAccessTokenSystemErrorMqo
{
    public required Guid CorrelationId { get; set; }

    public required string Message { get; set; } = string.Empty;
}
