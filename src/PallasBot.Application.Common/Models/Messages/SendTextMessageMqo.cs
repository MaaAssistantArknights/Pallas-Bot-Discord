namespace PallasBot.Application.Common.Models.Messages;

public record SendTextMessageMqo
{
    public required ulong ChannelId { get; init; }

    public required string Message { get; init; }
}
