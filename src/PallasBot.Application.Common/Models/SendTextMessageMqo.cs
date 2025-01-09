namespace PallasBot.Application.Common.Models;

public record SendTextMessageMqo
{
    public ulong ChannelId { get; init; }

    public string Message { get; init; } = string.Empty;
}
