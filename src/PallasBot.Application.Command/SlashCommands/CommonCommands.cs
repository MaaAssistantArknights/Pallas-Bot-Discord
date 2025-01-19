using Discord;
using Discord.Interactions;

namespace PallasBot.Application.Command.SlashCommands;

[CommandContextType(InteractionContextType.Guild)]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class CommonCommands : InteractionModuleBase
{
    [SlashCommand("ping", "Test the responsiveness of the bot")]
    public async Task PingAsync()
    {
        await RespondAsync("pong!", ephemeral: true);
    }
}
