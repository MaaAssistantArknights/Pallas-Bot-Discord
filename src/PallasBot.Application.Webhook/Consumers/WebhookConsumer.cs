using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using PallasBot.Application.Common.Abstract;
using PallasBot.Application.Common.Models;

namespace PallasBot.Application.Webhook.Consumers;

public class WebhookConsumer : IConsumer<WebhookMessage>
{
    private readonly IServiceProvider _serviceProvider;

    public WebhookConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Consume(ConsumeContext<WebhookMessage> context)
    {
        var m = context.Message;

        var webhookProcessor = _serviceProvider.GetKeyedService<IWebhookProcessor>(m.Processor.ToLowerInvariant());
        if (webhookProcessor is null)
        {
            return;
        }

        await webhookProcessor.ProcessAsync(m.Body);
    }
}
