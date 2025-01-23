namespace PallasBot.Application.Webhook.Models;

public record MaaReleaseMqo
{
    public required ulong ReleaseId { get; set; }

    public required DateTimeOffset ReleaseAt { get; set; }
}
