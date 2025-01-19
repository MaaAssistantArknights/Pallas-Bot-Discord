namespace PallasBot.Application.Common.Models.Messages.Jobs;

public record SyncGitHubOrganizationMqo
{
    public required bool SyncMembers { get; set; }
    public required string[] Repositories { get; set; }
}
