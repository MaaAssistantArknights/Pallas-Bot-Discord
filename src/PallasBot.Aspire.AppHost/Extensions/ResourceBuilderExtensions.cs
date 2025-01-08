namespace PallasBot.Aspire.AppHost.Extensions;

public static class ResourceBuilderExtensions
{
    public static string GetString(this IResourceBuilder<ParameterResource> parameterResourceBuilder)
    {
        var value = parameterResourceBuilder.Resource.Value;
        return value;
    }

    public static bool GetBool(this IResourceBuilder<ParameterResource> parameterResourceBuilder)
    {
        var value = parameterResourceBuilder.Resource.Value;
        return bool.Parse(value);
    }
}
