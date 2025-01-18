using Discord;
using Discord.Interactions;
using MassTransit;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Enums;

namespace PallasBot.Application.Command.SlashCommands;

[RequireOwner(Group = "Permission")]
[RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
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

        [SlashCommand("maa-team-role", "Set the role that will be given to MAA team members")]
        public async Task SetMaaTeamRole(
            [Summary(description: "The role that will be given to MAA team member.")] IRole role)
        {
            var guildId = Context.Guild.Id;

            await _dynamicConfigurationService.SetAsync(guildId, DynamicConfigurationKey.MaaTeamMemberRoleId, role.Id.ToString());

            await RespondAsync($"Set MAA team role to `{role.Name}`");
        }

        [SlashCommand("maa-contributor-role", "Set the role that will be given to MAA contributors")]
        public async Task SetMaaContributorRole(
            [Summary(description: "The role that will be given to MAA contributors.")] IRole role)
        {
            var guildId = Context.Guild.Id;

            await _dynamicConfigurationService.SetAsync(guildId, DynamicConfigurationKey.MaaContributorRoleId, role.Id.ToString());

            await RespondAsync($"Set MAA contributors role to `{role.Name}`");
        }
    }
}
