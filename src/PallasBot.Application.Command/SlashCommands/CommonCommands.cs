using Discord;
using Discord.Interactions;

namespace PallasBot.Application.Command.SlashCommands;

public class CommonCommands : InteractionModuleBase
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [SlashCommand("ping", "Test the responsiveness of the bot")]
    public async Task PingAsync()
    {
        await RespondAsync("pong!", ephemeral: true);
        throw new InvalidOperationException("Test exception");
    }
}
