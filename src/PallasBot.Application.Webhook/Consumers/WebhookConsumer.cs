using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PallasBot.Application.Common.Abstract;
using PallasBot.Application.Common.Models;

namespace PallasBot.Application.Webhook.Consumers;

public class WebhookConsumer : IConsumer<WebhookMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookConsumer> _logger;

    public WebhookConsumer(IServiceProvider serviceProvider, ILogger<WebhookConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WebhookMessage> context)
    {
        var m = context.Message;

        _logger.LogDebug("Received webhook message with processor {Processor}. Body: {Body}", m.Processor, m.Body);

        var webhookProcessor = _serviceProvider.GetKeyedService<IWebhookProcessor>(m.Processor.ToLowerInvariant());
        if (webhookProcessor is null)
        {
            _logger.LogWarning("No processor found for {Processor}", m.Processor);
            return;
        }

        await webhookProcessor.ProcessAsync(m.Body);
    }
}
