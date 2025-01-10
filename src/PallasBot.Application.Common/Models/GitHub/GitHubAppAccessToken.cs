using System.Text.Json.Serialization;

namespace PallasBot.Application.Common.Models.GitHub;

public record GitHubAppAccessToken
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
}
