using System.Text.Json.Serialization;
using FluentResults;

namespace PallasBot.Application.Common.Models.GitHub;

public record GitHubDeviceCodeFlowErrorResponse : IError
{
    [JsonPropertyName("error")]
    [JsonConverter(typeof(OAuthDeviceCodeFlowErrorJsonConverter))]
    public OAuthDeviceCodeFlowError Error { get; init; }

    [JsonIgnore]
    public string Message => Error.ToString();

    [JsonIgnore]
    public Dictionary<string, object> Metadata { get; } = [];

    [JsonIgnore]
    public List<IError> Reasons { get; } = [];
}
