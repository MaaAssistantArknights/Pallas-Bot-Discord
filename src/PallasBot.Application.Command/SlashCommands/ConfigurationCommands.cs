using Discord;
using Discord.Interactions;
using MassTransit;
using PallasBot.Application.Common.Models;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Enums;

namespace PallasBot.Application.Command.SlashCommands;

[Group("config", "Configuration commands")]
public class ConfigurationCommands : InteractionModuleBase
{
    [Group("set", "Set configurations")]
    public class ConfigurationSetPrefixCommands : InteractionModuleBase
    {
        private readonly IDynamicConfigurationService _dynamicConfigurationService;
        private readonly IPublishEndpoint _publishEndpoint;

        public ConfigurationSetPrefixCommands(IDynamicConfigurationService dynamicConfigurationService, IPublishEndpoint publishEndpoint)
        {
            _dynamicConfigurationService = dynamicConfigurationService;
            _publishEndpoint = publishEndpoint;
        }

        [SlashCommand("maa-release-channel", "Set which channel to post MAA releases")]
        public async Task SetMaaReleaseChannel(
            [Summary(description: "The channel to post MAA releases. Should be a Text channel."), ChannelTypes(ChannelType.Text)] IChannel channel)
        {
            var guildId = Context.Guild.Id;
            var channelId = channel.Id;

            await _dynamicConfigurationService.SetAsync(guildId, DynamicConfigurationKey.MaaReleaseNotificationChannel, channelId.ToString());

            await _publishEndpoint.Publish(new SendTextMessageMqo
            {
                ChannelId = channelId,
                Message = "This is a test message to check if the channel is set correctly. Please delete this message after confirming."
            });

            await RespondAsync($"Set MAA release channel to {channel.Name}, a test message will be sent to that channel.");
        }
    }
}
