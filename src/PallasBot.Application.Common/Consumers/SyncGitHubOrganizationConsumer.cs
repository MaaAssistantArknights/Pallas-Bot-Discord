using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Application.Common.Models.Messages.Jobs;
using PallasBot.Application.Common.Services;
using PallasBot.Domain.Constants;
using PallasBot.Domain.Entities;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Common.Consumers;

public class SyncGitHubOrganizationConsumer : IConsumer<SyncGitHubOrganizationMqo>
{
    private readonly ILogger<SyncGitHubOrganizationConsumer> _logger;
    private readonly GitHubApiService _gitHubApiService;
    private readonly PallasBotDbContext _dbContext;

    public SyncGitHubOrganizationConsumer(
        ILogger<SyncGitHubOrganizationConsumer> logger,
        GitHubApiService gitHubApiService,
        PallasBotDbContext dbContext)
    {
        _logger = logger;
        _gitHubApiService = gitHubApiService;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<SyncGitHubOrganizationMqo> context)
    {
        var m = context.Message;

        _logger.LogInformation("GitHub organization sync job started, repositories: {Repositories}",
            m.Repositories.Length == 0 ? "NULL" : string.Join(", ", m.Repositories));

        var accessToken = await _gitHubApiService.GetGitHubAppAccessTokenAsync();

        var changes = new List<string>();

        if (m.SyncMembers)
        {
            var syncMembersChanges = await SyncMembersAsync(accessToken.Token);
            changes.AddRange(syncMembersChanges);
        }

        foreach (var repository in m.Repositories)
        {
            var syncRepoChanges = await SyncRepositoryAsync(repository, accessToken.Token);
            changes.AddRange(syncRepoChanges);
        }

        changes = changes.Distinct().ToList();

        var msg = await EntityFrameworkQueryableExtensions.ToListAsync(_dbContext.DiscordUserBindings
                .Where(x => changes.Contains(x.GitHubLogin))
                .Select(x => new TryAssignMaaRoleMqo
                {
                    GuildId = x.GuildId,
                    UserId = x.DiscordUserId
                }));

        _logger.LogInformation("GitHub organization sync job completed, unique changes: {Changes}, role assign mqo sent: {MqoCount}",
            changes.Count, msg.Count);

        await context.PublishBatch(msg);
    }

    private async Task<List<string>> SyncMembersAsync(string accessToken)
    {
        var members = await _gitHubApiService.GetOrganizationMembersAsync(MaaConstants.Organization, accessToken);
        var organizationMembers = members
            .Select(x => new GitHubOrganizationMember
            {
                GitHubLogin = x.Login
            })
            .ToList();

        var removedQuery = _dbContext.GitHubOrganizationMembers
            .Where(x => organizationMembers.Contains(x) == false);

        var removedUsers = await EntityFrameworkQueryableExtensions.ToListAsync(removedQuery
                .AsNoTracking());
        await removedQuery.ExecuteDeleteAsync();

        var inserted = await _dbContext.GitHubOrganizationMembers
            .UpsertRange(organizationMembers)
            .NoUpdate()
            .RunAndReturnAsync();

        return removedUsers
            .Select(x => x.GitHubLogin)
            .Concat(inserted.Select(i => i.GitHubLogin))
            .ToList();
    }

    private async Task<List<string>> SyncRepositoryAsync(string repository, string accessToken)
    {
        var contributors = await _gitHubApiService
            .GetRepoContributorsAsync(MaaConstants.Organization, repository, accessToken);
        var repositoryContributors = contributors
            .Select(x => new GitHubContributor
            {
                GitHubLogin = x.Login,
                Repository = repository
            })
            .ToList();
        var githubLogins = repositoryContributors
            .Select(x => x.GitHubLogin)
            .ToList();

        var removedQuery = _dbContext.GitHubContributors
            .Where(x => x.Repository == repository)
            .Where(x => githubLogins.Contains(x.GitHubLogin) == false);

        var removedUsers = await removedQuery
            .AsNoTracking()
            .ToListAsync();
        await removedQuery.ExecuteDeleteAsync();

        var inserted = await _dbContext.GitHubContributors
            .UpsertRange(repositoryContributors)
            .NoUpdate()
            .RunAndReturnAsync();

        return removedUsers
            .Select(x => x.GitHubLogin)
            .Concat(inserted.Select(i => i.GitHubLogin))
            .ToList();
    }
}

public class SyncGitHubOrganizationConsumerDefinition : ConsumerDefinition<SyncGitHubOrganizationConsumer>
{
    public SyncGitHubOrganizationConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }
}
