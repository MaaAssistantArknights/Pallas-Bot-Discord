using Discord;
using Discord.Interactions;

namespace PallasBot.Application.Command.SlashCommands;

public class CommonCommands : InteractionModuleBase
{
    [RequireOwner(Group = "Permission")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [SlashCommand("ping", "Test the responsiveness of the bot")]
    public async Task PingAsync()
    {
        await RespondAsync("pong!", ephemeral: true);
    }
}
