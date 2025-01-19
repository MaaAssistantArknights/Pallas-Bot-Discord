using System.Reflection;
using Discord;
using PallasBot.Domain.Attributes;
using PallasBot.Domain.Enums;

namespace PallasBot.Domain.Extensions;

public static class DynamicConfigurationExtensions
{
    public static string Format(this DynamicConfigurationKey key, string source)
    {
        var fieldInfo = key.GetType().GetField(key.ToString());

        var attr = fieldInfo?.GetCustomAttribute<DynamicConfigurationTypeAttribute>();

        if (attr is null)
        {
            return source;
        }

        return attr.Type switch
        {
            DynamicConfigurationType.Role => MentionUtils.MentionRole(ulong.Parse(source)),
            DynamicConfigurationType.Channel => MentionUtils.MentionChannel(ulong.Parse(source)),
            _ => source
        };
    }
}
