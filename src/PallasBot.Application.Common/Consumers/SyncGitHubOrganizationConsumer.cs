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

        var msg = await _dbContext.GitHubUserBindings
            .Where(x => changes.Contains(x.GitHubLogin))
            .Select(x => new TryAssignMaaRoleMqo
            {
                GuildId = x.GuildId,
                UserId = x.DiscordUserId
            })
            .ToListAsync();

        _logger.LogInformation("GitHub organization sync job completed, unique changes: {Changes}, role assign mqo sent: {MqoCount}",
            changes.Count, msg.Count);

        await context.PublishBatch(msg);
    }

    private async Task<List<string>> SyncMembersAsync(string accessToken)
    {
        var members = await _gitHubApiService.GetOrganizationMembersAsync(MaaConstants.Organization, accessToken);

        var userLogins = members.Select(x => x.Login).ToList();

        var changes = new List<string>();

        var existingUsers = await _dbContext.GitHubContributors
            .Where(x => userLogins.Contains(x.GitHubLogin) && x.IsOrganizationMember == false)
            .ToListAsync();

        foreach (var login in userLogins)
        {
            var user = existingUsers.FirstOrDefault(x => x.GitHubLogin == login);

            if (user is null)
            {
                _dbContext.GitHubContributors.Add(new GitHubContributor
                {
                    GitHubLogin = login,
                    IsOrganizationMember = true
                });
            }
            else
            {
                user.IsOrganizationMember = true;
                _dbContext.GitHubContributors.Update(user);
            }

            changes.Add(login);
        }

        var existingUsersToRemove = await _dbContext.GitHubContributors
            .Where(x => userLogins.Contains(x.GitHubLogin) == false && x.IsOrganizationMember)
            .ToListAsync();
        foreach (var user in existingUsersToRemove)
        {
            user.IsOrganizationMember = false;
            _dbContext.GitHubContributors.Update(user);
            changes.Add(user.GitHubLogin);
        }

        await _dbContext.SaveChangesAsync();

        return changes.Distinct().ToList();
    }

    private async Task<List<string>> SyncRepositoryAsync(string repository, string accessToken)
    {
        var contributors = await _gitHubApiService
            .GetRepoContributorsAsync(MaaConstants.Organization, repository, accessToken);

        var userLogins = contributors.Select(x => x.Login).ToList();

        var existingUsers = await _dbContext.GitHubContributors
            .Where(x => userLogins.Contains(x.GitHubLogin) && x.ContributeTo.Contains(repository) == false)
            .ToListAsync();

        var changes = new List<string>();
        foreach (var login in userLogins)
        {
            var user = existingUsers.FirstOrDefault(x => x.GitHubLogin == login);

            if (user is null)
            {
                var c = new GitHubContributor
                {
                    GitHubLogin = login,
                };
                c.AddContribution(repository);
                await _dbContext.GitHubContributors.AddAsync(c);
            }
            else
            {
                user.AddContribution(repository);
                _dbContext.GitHubContributors.Update(user);
            }

            changes.Add(login);
        }

        var existingUsersToRemove = await _dbContext.GitHubContributors
            .Where(x => userLogins.Contains(x.GitHubLogin) == false && x.ContributeTo.Contains(repository))
            .ToListAsync();
        foreach (var user in existingUsersToRemove)
        {
            user.RemoveContribution(repository);
            _dbContext.GitHubContributors.Update(user);
            changes.Add(user.GitHubLogin);
        }

        await _dbContext.SaveChangesAsync();

        return changes.Distinct().ToList();
    }
}

public class SyncGitHubOrganizationConsumerDefinition : ConsumerDefinition<SyncGitHubOrganizationConsumer>
{
    public SyncGitHubOrganizationConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }
}
