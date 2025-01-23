using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace PallasBot.Application.Common.Services;

public class AiService
{
    private readonly IServiceProvider _serviceProvider;

    public AiService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ChatCompletion> CompleteAsync(
        string clientName,
        List<ChatMessage> chatMessages,
        ChatOptions? chatOptions = null,
        CancellationToken cancellationToken = new())
    {
        var client = GetClient(clientName);

        return await client.CompleteAsync(chatMessages, chatOptions, cancellationToken);
    }

    private IChatClient GetClient(string clientName)
    {
        return _serviceProvider.GetKeyedService<IChatClient>(clientName) ??
               _serviceProvider.GetRequiredKeyedService<IChatClient>("Default");
    }
}
