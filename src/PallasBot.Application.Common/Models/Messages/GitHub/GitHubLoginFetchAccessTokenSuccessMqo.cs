namespace PallasBot.Application.Common.Models.Messages.GitHub;

public record GitHubLoginFetchAccessTokenSuccessMqo
{
    public required Guid CorrelationId { get; set; }

    public required string AccessToken { get; set; }
}
