using System.Diagnostics;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Abstract;
using PallasBot.Application.Common.Models;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Constants;

namespace PallasBot.Application.Webhook.Consumers;

public class WebhookConsumer : IConsumer<WebhookMessageMqo>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookConsumer> _logger;

    public WebhookConsumer(IServiceProvider serviceProvider, ILogger<WebhookConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WebhookMessageMqo> context)
    {
        var m = context.Message;

        Activity.Current?.AddTag("webhook.processor.name", m.Processor);

        _logger.LogDebug("Received webhook message with processor {Processor}. Body: {Body}", m.Processor, m.Body);

        var webhookProcessor = _serviceProvider.GetKeyedService<IWebhookProcessor>(m.Processor.ToLowerInvariant());
        if (webhookProcessor is null)
        {
            _logger.LogWarning("No processor found for {Processor}", m.Processor);
            Activity.Current?.AddTag("webhook.processor.status", "unknown");
            return;
        }
        Activity.Current?.AddTag("webhook.processor.status", "ok");

        using var activity = ActivitySources.WebhookProcessorActivitySource.StartActivity($"WebhookProcess: {m.Processor}");

        try
        {
            await webhookProcessor.ProcessAsync(m);
        }
        catch (Exception e)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(e);
        }
    }
}
