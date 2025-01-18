namespace PallasBot.Application.Common.Models.Messages.GitHub;

public record GitHubLoginBindingUserMqo
{
    public required Guid CorrelationId { get; set; }

    public required ulong GuildId { get; set; }

    public required ulong DiscordUserId { get; set; }

    public required string AccessToken { get; set; } = string.Empty;
}
