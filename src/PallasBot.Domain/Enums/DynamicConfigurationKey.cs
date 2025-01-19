using PallasBot.Domain.Attributes;

namespace PallasBot.Domain.Enums;

public enum DynamicConfigurationKey
{
    [DynamicConfigurationType(DynamicConfigurationType.Channel)]
    MaaReleaseNotificationChannel,

    [DynamicConfigurationType(DynamicConfigurationType.Role)]
    MaaTeamMemberRoleId,

    [DynamicConfigurationType(DynamicConfigurationType.Role)]
    MaaContributorRoleId,
}

public enum DynamicConfigurationType
{
    Role,
    Channel
}
