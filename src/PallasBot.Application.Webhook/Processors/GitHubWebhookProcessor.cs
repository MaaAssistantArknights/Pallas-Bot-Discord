using System.Diagnostics;
using System.Text.Json;
using MassTransit;
using PallasBot.Application.Common.Abstract;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Application.Webhook.Models;
using PallasBot.Application.Webhook.Services;

namespace PallasBot.Application.Webhook.Processors;

public class GitHubWebhookProcessor : IWebhookProcessor
{
    private readonly GitHubWebhookValidator _gitHubWebhookValidator;
    private readonly IPublishEndpoint _publishEndpoint;

    public GitHubWebhookProcessor(
        GitHubWebhookValidator gitHubWebhookValidator, IPublishEndpoint publishEndpoint)
    {
        _gitHubWebhookValidator = gitHubWebhookValidator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task ProcessAsync(WebhookMessageMqo messageMqo)
    {
        var eventType = messageMqo.GetHeader(WebhookMessageMqo.HeaderEventType);
        var deliveryId = messageMqo.GetHeader(WebhookMessageMqo.HeaderDeliveryId);
        var signature = messageMqo.GetHeader(WebhookMessageMqo.HeaderSignature);

        Activity.Current?.AddTag("webhook.github.event-type", eventType);
        Activity.Current?.AddTag("webhook.github.delivery-id", deliveryId);
        Activity.Current?.AddTag("webhook.github.signature", signature);

        var validationResult = await _gitHubWebhookValidator.ValidateAsync(messageMqo.Body, signature);
        if (validationResult.IsFailed)
        {
            throw new InvalidOperationException($"GitHub webhook validation failed. {string.Join(',', validationResult.Errors.Select(x => x.ToString()))}");
        }

        switch (eventType)
        {
            case "ping":
                return;
            case "release":
                await ProcessReleaseEventAsync(messageMqo.Body);
                return;
            default:
                return;
        }
    }

    private async Task ProcessReleaseEventAsync(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var type = root.GetProperty("action").GetString();
        if (type != "published")
        {
            return;
        }

        var release = root.GetProperty("release");

        var id = release.GetProperty("id").GetUInt64();
        var publishedAt = release.GetProperty("published_at").GetDateTimeOffset();
        await _publishEndpoint.Publish(new MaaReleaseMqo
        {
            ReleaseId = id,
            ReleaseAt = publishedAt
        });
    }
}
