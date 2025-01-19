using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Models.GitHub;
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

        var members = await _gitHubApiService.GetOrganizationMembersAsync(MaaConstants.Organization, accessToken.Token);
        var contributors = new List<GitHubUser>();
        foreach (var repository in m.Repositories)
        {
            var repositoryContributors = await _gitHubApiService.GetRepoContributorsAsync(
                MaaConstants.Organization, repository, accessToken.Token);
            contributors.AddRange(repositoryContributors);
        }
        contributors = contributors.DistinctBy(x => x.Login).ToList();

        var logins = members.Select(x => x.Login)
            .Concat(contributors.Select(x => x.Login))
            .Distinct()
            .ToList();

        var existingUsers = await _dbContext.GitHubContributors
            .Where(x => logins.Contains(x.GitHubLogin))
            .ToListAsync();

        var changes = new List<string>();
        foreach (var login in logins)
        {
            var isNew = false;
            var user = existingUsers.FirstOrDefault(x => x.GitHubLogin == login);
            if (user is null)
            {
                isNew = true;
                changes.Add(login);
                user = new GitHubContributor
                {
                    GitHubLogin = login,
                };
            }

            var member = members.FirstOrDefault(x => x.Login == login);
            if (member is not null)
            {
                if (user.IsOrganizationMember is false)
                {
                    changes.Add(member.Login);
                }
                user.IsOrganizationMember = true;
            }

            var contributor = contributors.FirstOrDefault(x => x.Login == login);
            if (contributor is not null)
            {
                if (user.IsContributor is false)
                {
                    changes.Add(contributor.Login);
                }
                user.IsContributor = true;
            }

            if (isNew)
            {
                await _dbContext.GitHubContributors.AddAsync(user);
            }
            else
            {
                _dbContext.GitHubContributors.Update(user);
            }
        }

        await _dbContext.SaveChangesAsync();

        var msg = await _dbContext.GitHubUserBindings
            .Where(x => changes.Contains(x.GitHubLogin))
            .Select(x => new TryAssignMaaRoleMqo
            {
                GuildId = x.GuildId,
                UserId = x.DiscordUserId
            })
            .ToListAsync();

        await context.PublishBatch(msg);
    }
}
