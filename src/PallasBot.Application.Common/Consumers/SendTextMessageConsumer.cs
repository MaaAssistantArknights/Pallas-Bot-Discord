using Discord.Rest;
using MassTransit;
using PallasBot.Application.Common.Models.Messages;

namespace PallasBot.Application.Common.Consumers;

public class SendMessageConsumer : IConsumer<SendTextMessageMqo>
{
    private readonly DiscordRestClient _discordRestClient;

    public SendMessageConsumer(DiscordRestClient discordRestClient)
    {
        _discordRestClient = discordRestClient;
    }

    public async Task Consume(ConsumeContext<SendTextMessageMqo> context)
    {
        var m = context.Message;

        var channel = (IRestMessageChannel) await _discordRestClient.GetChannelAsync(m.ChannelId);

        await channel.SendMessageAsync(text: m.Message);
    }
}
