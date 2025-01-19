using MassTransit;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Models.Messages.Jobs;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Constants;

namespace PallasBot.Application.Common.Jobs;

public class SyncGitHubOrganizationJob : ScopedTimedBackgroundWorker<IPublishEndpoint>
{
    public SyncGitHubOrganizationJob(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        : base(new ScopedTimedBackgroundWorkerOptions
        {
            RunOnStart = false,
            Interval = TimeSpan.FromHours(12)
        }, loggerFactory, serviceProvider)
    {
    }

    protected override string Name => "sync-github-org";

    protected override async Task ExecuteAsync(IPublishEndpoint service, CancellationToken cancellationToken)
    {
        await service.Publish(new SyncGitHubOrganizationMqo
        {
            SyncMembers = true,
            Repositories = MaaConstants.Repositories.ToArray()
        }, cancellationToken);
    }
}
