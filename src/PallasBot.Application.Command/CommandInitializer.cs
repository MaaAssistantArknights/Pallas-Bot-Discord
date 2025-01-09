using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Attributes;
using PallasBot.Domain.Constants;
using PallasBot.Domain.Exceptions;

namespace PallasBot.Application.Command;

public class CommandInitializer : IDiscordApplicationInitializer
{
    private readonly InteractionService _interactionService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandInitializer> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public CommandInitializer(
        InteractionService interactionService,
        DiscordSocketClient discordSocketClient,
        IServiceProvider serviceProvider,
        ILogger<CommandInitializer> logger,
        IHostEnvironment hostEnvironment)
    {
        _interactionService = interactionService;
        _discordSocketClient = discordSocketClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public Task SocketInitializer(DiscordSocketClient discordSocketClient)
    {
        discordSocketClient.SlashCommandExecuted += HandleSlashCommandAsync;
        discordSocketClient.AutocompleteExecuted += HandleAutoCompleteAsync;

        return Task.CompletedTask;
    }

    public Task RestInitializer(DiscordRestClient discordRestClient)
    {
        discordRestClient.LoggedIn += async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var registeredModules = await _interactionService.AddModulesAsync(typeof(CommandInitializer).Assembly, scope.ServiceProvider);
            foreach (var module in registeredModules)
            {
                var hasDevOnly = module.Attributes.FirstOrDefault(x => x is DevOnlyAttribute);
                if (hasDevOnly is not null && _hostEnvironment.IsDevelopment() is false)
                {
                    await _interactionService.RemoveModuleAsync(module);
                }
            }

            foreach (var module in _interactionService.Modules)
            {
                _logger.LogInformation("Interaction module registered: {ModuleName}", module.Name);
            }

            await _interactionService.RegisterCommandsGloballyAsync();
        };

        return Task.CompletedTask;
    }


    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private async Task HandleSlashCommandAsync(SocketSlashCommand interaction)
    {
        using var activity = ActivitySources.CommandActivitySource.StartActivity(interaction.CommandName, ActivityKind.Server);

        activity?.AddTag("interaction_id", interaction.Id.ToString());
        activity?.AddTag("command_name", interaction.CommandName);
        activity?.AddTag("command_id", interaction.CommandId.ToString());
        activity?.AddTag("user_id", interaction.User?.Id.ToString());
        activity?.AddTag("guild_id", interaction.GuildId?.ToString());
        activity?.AddTag("channel_id", interaction.Channel?.Id.ToString());

        var ctx = new SocketInteractionContext<SocketSlashCommand>(_discordSocketClient, interaction);

        activity?.AddEvent(new ActivityEvent("Execute"));

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var result = await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            if (result.IsSuccess is false)
            {
                throw new InteractionFailedException(result);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing command {CommandName}", interaction.CommandName);

            activity?.AddException(e);
            activity?.SetStatus(ActivityStatusCode.Error);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private async Task HandleAutoCompleteAsync(SocketAutocompleteInteraction interaction)
    {
        var commandName = interaction.Data?.CommandName ?? "null";
        using var activity = ActivitySources.CommandAutocompletionActivitySource.StartActivity(commandName, ActivityKind.Server);

        activity?.AddTag("interaction_id", interaction.Id.ToString());
        activity?.AddTag("command_name", interaction.Data?.CommandName);
        activity?.AddTag("command_id", interaction.Data?.CommandId.ToString());
        activity?.AddTag("user_id", interaction.User?.Id.ToString());
        activity?.AddTag("guild_id", interaction.GuildId?.ToString());
        activity?.AddTag("channel_id", interaction.Channel?.Id.ToString());

        var ctx = new SocketInteractionContext<SocketAutocompleteInteraction>(_discordSocketClient, interaction);

        activity?.AddEvent(new ActivityEvent("Execute"));

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var result = await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            if (result.IsSuccess is false)
            {
                throw new InteractionFailedException(result);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing auto complete {CommandName}", commandName);

            activity?.AddException(e);
            activity?.SetStatus(ActivityStatusCode.Error);
        }
    }
}
