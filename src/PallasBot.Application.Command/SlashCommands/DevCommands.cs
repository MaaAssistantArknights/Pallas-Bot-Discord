using Discord;
using Discord.Interactions;
using MassTransit;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Attributes;

namespace PallasBot.Application.Command.SlashCommands;

[RequireOwner]
[DevOnly]
[Group("dev", "Development commands")]
public class DevCommands : InteractionModuleBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public DevCommands(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [SlashCommand("publish-assign-maa-role", "Publish TryAssignMaaRoleMqo.")]
    public async Task TestPublishAssignMaaRole(
        [Summary(description: "The user that trigger the assign maa role mqo.")] IUser user)
    {
        var mqo = new TryAssignMaaRoleMqo
        {
            GuildId = Context.Guild.Id,
            UserId = user.Id
        };

        await _publishEndpoint.Publish(mqo);

        await RespondAsync($"Message published: {mqo}");
    }
}
