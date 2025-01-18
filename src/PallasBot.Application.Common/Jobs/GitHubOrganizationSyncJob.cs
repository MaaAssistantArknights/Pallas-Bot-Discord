using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Models.GitHub;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Application.Common.Services;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Entities;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Common.Jobs;

public class GitHubOrganizationSyncJob : ScopedTimedBackgroundWorker
{
    public GitHubOrganizationSyncJob(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        : base(
            new ScopedTimedBackgroundWorkerOptions
            {
                Interval = TimeSpan.FromHours(4),
                RunOnStart = false
            },
            loggerFactory, serviceProvider)
    {
    }

    protected override string Name => nameof(GitHubOrganizationSyncJob);

    protected override async Task ExecuteInScopeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var gitHubApiService = serviceProvider.GetRequiredService<GitHubApiService>();
        var dbContext = serviceProvider.GetRequiredService<PallasBotDbContext>();
        var publishEndpoint = serviceProvider.GetRequiredService<IPublishEndpoint>();

        Logger.LogInformation("GitHub organization sync job started");

        var accessToken = await gitHubApiService.GetGitHubAppAccessTokenAsync();

        var members = await gitHubApiService.GetOrganizationMembersAsync("MaaAssistantArknights", accessToken.Token);
        var contributors = await gitHubApiService.GetRepoContributorsAsync("MaaAssistantArknights", "MaaAssistantArknights", accessToken.Token);

        var changes = await PersistGitHubOrganizationInfoAsync(dbContext, members, contributors);

        var msg = await dbContext.GitHubUserBindings
            .Where(x => changes.Contains(x.GitHubLogin))
            .Select(x => new TryAssignMaaRoleMqo
            {
                GuildId = x.GuildId,
                UserId = x.DiscordUserId
            })
            .ToListAsync(cancellationToken);

        await publishEndpoint.PublishBatch(msg, cancellationToken);
    }

    private static async Task<List<string>> PersistGitHubOrganizationInfoAsync(PallasBotDbContext dbContext, List<GitHubUser> members, List<GitHubUser> contributors)
    {
        var logins = members.Select(x => x.Login)
            .Concat(contributors.Select(x => x.Login))
            .Distinct()
            .ToList();

        var existingUsers = await dbContext.GitHubContributors
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
                if (user.IsTeamMember is false)
                {
                    changes.Add(member.Login);
                }
                user.IsTeamMember = true;
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
                await dbContext.GitHubContributors.AddAsync(user);
            }
            else
            {
                dbContext.GitHubContributors.Update(user);
            }
        }

        await dbContext.SaveChangesAsync();

        return changes.Distinct().ToList();
    }
}
