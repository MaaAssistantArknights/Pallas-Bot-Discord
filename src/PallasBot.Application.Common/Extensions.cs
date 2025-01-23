using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;
using PallasBot.Application.Common.Jobs;
using PallasBot.Application.Common.Options;
using PallasBot.Application.Common.Services;
using PallasBot.Domain.Constants;

namespace PallasBot.Application.Common;

public static class Extensions
{
    public static void AddApplicationCommonServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<GitHubApiService>();

        builder.Services.AddHostedService<SyncGitHubOrganizationJob>();

        builder.AddChatBot();
    }

    private static void AddChatBot(this IHostApplicationBuilder builder)
    {
        var options = AiOptions.Get(builder.Configuration);

        builder.Services.AddSingleton<OpenAIClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

            var httpClient = httpClientFactory.CreateClient("OpenRouterAI");
            var transport = new HttpClientPipelineTransport(httpClient);

            var client = new OpenAIClient(
                new ApiKeyCredential(options.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(options.Endpoint),
                    Transport = transport
                });

            return client;
        });

        var models = options.Models;

        var chatClientService = new Dictionary<string, string>
        {
            ["Default"] = models.Default,
            ["ChangelogSummary"] = string.IsNullOrEmpty(models.ChangelogSummary)
                ? models.Default
                : models.ChangelogSummary
        };

        foreach (var (key, model) in chatClientService)
        {
            builder.Services.AddKeyedChatClient(key, sp =>
            {
                if (key != "Default" && model == chatClientService["Default"])
                {
                    return sp.GetRequiredKeyedService<IChatClient>("Default");
                }

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var openAiClient = sp.GetRequiredService<OpenAIClient>();
                var chatClient = new ChatClientBuilder(openAiClient.AsChatClient(model))
                    .UseLogging(loggerFactory)
                    .UseOpenTelemetry(loggerFactory, ActivitySources.AppAiActivitySource.Name, otel =>
                    {
                        otel.EnableSensitiveData = builder.Environment.IsDevelopment();
                    })
                    .Build();

                return chatClient;
            });
        }

        builder.Services.AddScoped<AiService>();
    }
}
