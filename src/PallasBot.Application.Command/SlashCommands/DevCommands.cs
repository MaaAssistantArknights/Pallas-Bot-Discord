using Discord;
using Discord.Interactions;
using PallasBot.Domain.Attributes;

namespace PallasBot.Application.Command.SlashCommands;

[DevOnly]
[RequireOwner]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class DevCommands : InteractionModuleBase
{
    [SlashCommand("throw-error", "Throws an error")]
    public async Task ThrowError()
    {
        await RespondAsync("Will throw an error", ephemeral: true);
        throw new InvalidOperationException("This is a test error");
    }
}
