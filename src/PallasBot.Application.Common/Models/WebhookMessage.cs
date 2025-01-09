using Microsoft.Extensions.Primitives;

namespace PallasBot.Application.Common.Models;

public record WebhookMessage
{
    public string Processor { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public List<KeyValuePair<string, string[]>> Headers { get; set; } = [];
}
