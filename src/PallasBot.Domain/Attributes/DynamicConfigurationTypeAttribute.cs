using PallasBot.Domain.Enums;

namespace PallasBot.Domain.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class DynamicConfigurationTypeAttribute : Attribute
{
    public DynamicConfigurationType Type { get; }

    public DynamicConfigurationTypeAttribute(DynamicConfigurationType type)
    {
        Type = type;
    }
}
