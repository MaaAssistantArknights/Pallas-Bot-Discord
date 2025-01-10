using System.Text.Json.Serialization;

namespace PallasBot.Application.Common.Models.GitHub;

public record GitHubUser
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
