using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Services;
using PallasBot.Domain.Abstract;

namespace PallasBot.Application.Common.Jobs;

public class GitHubOrganizationSyncJob : ScopedTimedBackgroundWorker
{
    public GitHubOrganizationSyncJob(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        : base(
            new ScopedTimedBackgroundWorkerOptions
            {
                Interval = TimeSpan.FromHours(4),
                RunOnStart = true
            },
            loggerFactory, serviceProvider)
    {
    }

    protected override string Name => nameof(GitHubOrganizationSyncJob);

    protected override async Task ExecuteInScopeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var gitHubApiService = serviceProvider.GetRequiredService<GitHubApiService>();

        Logger.LogInformation("GitHub organization sync job started");

        var accessToken = await gitHubApiService.GetGitHubAppAccessToken();

        var users = await gitHubApiService.GetOrganizationMembers("MaaAssistantArknights", accessToken.Token);

        // TODO: Persist
        // TODO: Offer Discord Roles

        Logger.LogInformation("User count: {UserCount}", users.Count);
    }
}
