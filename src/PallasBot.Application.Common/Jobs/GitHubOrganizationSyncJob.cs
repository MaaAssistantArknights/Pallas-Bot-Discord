using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Models.GitHub;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Application.Common.Services;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Entities;
using PallasBot.Domain.Extensions;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.Application.Common.Jobs;

public class GitHubOrganizationSyncJob : ScopedTimedBackgroundWorker<GitHubOrganizationSyncService>
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

    protected override async Task ExecuteAsync(GitHubOrganizationSyncService service, CancellationToken cancellationToken)
    {
        await service.SyncOrganizationAsync(cancellationToken);
    }
}

public class GitHubOrganizationSyncService
{
    private readonly GitHubApiService _gitHubApiService;
    private readonly PallasBotDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GitHubOrganizationSyncService> _logger;

    private const string OrganizationName = "MaaAssistantArknights";
    private const string RepositoryName = "MaaAssistantArknights";

    public GitHubOrganizationSyncService(
        GitHubApiService gitHubApiService,
        IPublishEndpoint publishEndpoint,
        PallasBotDbContext dbContext,
        ILogger<GitHubOrganizationSyncService> logger)
    {
        _gitHubApiService = gitHubApiService;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SyncOrganizationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GitHub organization sync job started");

        var accessToken = await _gitHubApiService.GetGitHubAppAccessTokenAsync();

        var members = await _gitHubApiService.GetOrganizationMembersAsync(OrganizationName, accessToken.Token);
        var contributors = await _gitHubApiService.GetRepoContributorsAsync(OrganizationName, RepositoryName, accessToken.Token);

        var changes = await PersistGitHubOrganizationInfoAsync(members, contributors);

        var msg = await _dbContext.GitHubUserBindings
            .Where(x => changes.Contains(x.GitHubLogin))
            .Select(x => new TryAssignMaaRoleMqo
            {
                GuildId = x.GuildId,
                UserId = x.DiscordUserId
            })
            .ToListAsync(cancellationToken);

        await _publishEndpoint.PublishBatch(msg, cancellationToken);
    }

    private async Task<List<string>> PersistGitHubOrganizationInfoAsync(List<GitHubUser> members, List<GitHubUser> contributors)
    {
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
                await _dbContext.GitHubContributors.AddAsync(user);
            }
            else
            {
                _dbContext.GitHubContributors.Update(user);
            }
        }

        await _dbContext.SaveChangesAsync();

        return changes.Distinct().ToList();
    }
}
