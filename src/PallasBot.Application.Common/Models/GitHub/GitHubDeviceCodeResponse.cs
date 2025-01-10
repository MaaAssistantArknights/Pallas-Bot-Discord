using System.Text.Json.Serialization;

namespace PallasBot.Application.Common.Models.GitHub;

public record GitHubDeviceCodeResponse
{
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; } = string.Empty;

    [JsonPropertyName("user_code")]
    public string UserCode { get; set; } = string.Empty;

    [JsonPropertyName("verification_uri")]
    public string VerificationUri { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }
}
