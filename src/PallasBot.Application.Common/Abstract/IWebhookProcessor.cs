using PallasBot.Application.Common.Models;

namespace PallasBot.Application.Common.Abstract;

public interface IWebhookProcessor
{
    public Task ProcessAsync(WebhookMessageMqo messageMqo);
}
