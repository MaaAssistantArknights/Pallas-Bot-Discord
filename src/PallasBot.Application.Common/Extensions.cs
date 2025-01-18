using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PallasBot.Application.Common.Jobs;
using PallasBot.Application.Common.Services;
using PallasBot.Domain.Extensions;

namespace PallasBot.Application.Common;

public static class Extensions
{
    public static void AddApplicationCommonServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<GitHubApiService>();

        builder.Services.AddScopedTimedBackgroundWorker<GitHubOrganizationSyncJob, GitHubOrganizationSyncService>();
    }
}
