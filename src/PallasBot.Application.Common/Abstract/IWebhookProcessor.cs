using PallasBot.Application.Common.Models;
using PallasBot.Application.Common.Models.Messages;

namespace PallasBot.Application.Common.Abstract;

public interface IWebhookProcessor
{
    public Task ProcessAsync(WebhookMessageMqo messageMqo);
}
