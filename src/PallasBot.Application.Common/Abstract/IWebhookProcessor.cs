namespace PallasBot.Application.Common.Abstract;

public interface IWebhookProcessor
{
    public Task ProcessAsync(string content);
}
