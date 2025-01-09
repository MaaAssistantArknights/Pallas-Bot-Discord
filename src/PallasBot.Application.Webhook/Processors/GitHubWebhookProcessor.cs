using PallasBot.Application.Common.Abstract;

namespace PallasBot.Application.Webhook.Processors;

public class GitHubWebhookProcessor : IWebhookProcessor
{
    public Task ProcessAsync(string content)
    {
        return Task.CompletedTask;
    }
}
