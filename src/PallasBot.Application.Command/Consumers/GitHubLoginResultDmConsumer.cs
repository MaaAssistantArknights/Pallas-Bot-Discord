using Discord;
using Discord.Rest;
using MassTransit;
using PallasBot.Application.Common.Models.Messages.GitHub;

namespace PallasBot.Application.Command.Consumers;

public class GitHubLoginResultDmConsumer : IConsumer<GitHubLoginResultDmMqo>
{
    private readonly DiscordRestClient _discordRestClient;

    public GitHubLoginResultDmConsumer(DiscordRestClient discordRestClient)
    {
        _discordRestClient = discordRestClient;
    }

    public async Task Consume(ConsumeContext<GitHubLoginResultDmMqo> context)
    {
        var m = context.Message;

        var discordUser = await _discordRestClient.GetUserAsync(m.DiscordUserId);

        await discordUser.SendMessageAsync(m.TextMessage);

        await context.Publish(new GitHubLoginResultDmOkMqo
        {
            CorrelationId = m.CorrelationId
        });
    }
}
