using System.Text.Json.Serialization;

namespace PallasBot.Application.Common.Models.GitHub;

public record GitHubDeviceCodeAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
