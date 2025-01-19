using Discord;
using Discord.Interactions;
using MassTransit;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Enums;
using PallasBot.Domain.Extensions;

namespace PallasBot.Application.Command.SlashCommands;

[RequireOwner(Group = "Permission")]
[RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
[Group("config", "Configuration commands")]
public class ConfigurationCommands : InteractionModuleBase
{
    private readonly IDynamicConfigurationService _dynamicConfigurationService;

    public ConfigurationCommands(IDynamicConfigurationService dynamicConfigurationService)
    {
        _dynamicConfigurationService = dynamicConfigurationService;
    }

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
            var invoker = Context.User.Id;

            await _dynamicConfigurationService.SetAsync(guildId, DynamicConfigurationKey.MaaReleaseNotificationChannel, channelId.ToString(), invoker);

            await _publishEndpoint.Publish(new SendTextMessageMqo
            {
                ChannelId = channelId,
                Message = "This is a test message to check if the channel is set correctly. Please delete this message after confirming."
            });

            await RespondAsync($"Set MAA release channel to {channel.Name}, a test message will be sent to that channel.");
        }

        [SlashCommand("maa-member-role", "Set the role that will be given to MAA organization members")]
        public async Task SetMaaTeamRole(
            [Summary(description: "The role that will be given to MAA organization member.")] IRole role)
        {
            var guildId = Context.Guild.Id;
            var invoker = Context.User.Id;

            await _dynamicConfigurationService.SetAsync(guildId, DynamicConfigurationKey.MaaOrganizationMemberRoleId, role.Id.ToString(), invoker);

            await RespondAsync($"Set MAA organization role to `{role.Name}`");
        }

        [SlashCommand("maa-contributor-role", "Set the role that will be given to MAA contributors")]
        public async Task SetMaaContributorRole(
            [Summary(description: "The role that will be given to MAA contributors.")] IRole role)
        {
            var guildId = Context.Guild.Id;
            var invoker = Context.User.Id;

            await _dynamicConfigurationService.SetAsync(guildId, DynamicConfigurationKey.MaaContributorRoleId, role.Id.ToString(), invoker);

            await RespondAsync($"Set MAA contributors role to `{role.Name}`");
        }
    }

    [SlashCommand("list", "List configurations in current guild")]
    public async Task GetConfigurationAsync()
    {
        var guildId = Context.Guild.Id;

        var config = await _dynamicConfigurationService.GetAllByGuildAsync(guildId);

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Current configurations")
            .WithDescription("Current configurations in this guild")
            .WithColor(Color.Blue);

        foreach (var (k, v) in config)
        {
            embedBuilder.AddField(k.ToString("G"), k.Format(v));
        }

        var embed = embedBuilder.Build();

        await RespondAsync(embed: embed);
    }
}
