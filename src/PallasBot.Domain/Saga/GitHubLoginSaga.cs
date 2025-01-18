using MassTransit;

namespace PallasBot.Domain.Saga;

public class GitHubLoginSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public string CurrentState { get; set; } = string.Empty;

    public ulong GuildId { get; set; }

    public ulong DiscordUserId { get; set; }

    public string DeviceCode { get; set; } = string.Empty;

    public string UserCode { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }

    public int Interval { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
}
