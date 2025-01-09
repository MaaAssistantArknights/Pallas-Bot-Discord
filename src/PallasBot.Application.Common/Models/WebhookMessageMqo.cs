namespace PallasBot.Application.Common.Models;

public record WebhookMessageMqo
{
    public string Processor { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public Dictionary<string, string[]> Headers { get; set; } = [];

    public string GetHeader(string key)
    {
        if (Headers.TryGetValue(key, out var value))
        {
            if (value.Length == 0)
            {
                return string.Empty;
            }

            return value[0];
        }

        return string.Empty;
    }
}
