namespace PallasBot.Application.Common.Models.Messages.Jobs;

public record SyncGitHubOrganizationMqo
{
    public required bool SyncMembers { get; set; }
    public required string[] Repositories { get; set; }

    public override string ToString()
    {
        var repositoriesFormatted = string.Join(", ", Repositories);
        return $"SyncGitHubOrganizationMqo {{ SyncMembers = {SyncMembers}, Repositories = [{repositoriesFormatted}] }}";
    }
}
