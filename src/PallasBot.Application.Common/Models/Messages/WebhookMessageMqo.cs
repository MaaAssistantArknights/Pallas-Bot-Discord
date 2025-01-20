namespace PallasBot.Application.Common.Models.Messages;

public record WebhookMessageMqo
{
    public required string Processor { get; init; }

    public required string Body { get; init; }

    public required Dictionary<string, string[]> Headers { get; set; }

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

    public const string HeaderEventType = "X-GitHub-Event";
    public const string HeaderDeliveryId = "X-GitHub-Delivery";
    public const string HeaderSignature = "X-Hub-Signature-256";
}
