using Microsoft.Extensions.Configuration;
using PallasBot.Domain.Abstract;

namespace PallasBot.Application.Common.Options;

public record AiOptions : IOptionType<AiOptions>
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public AiModelOptions Models { get; set; } = new();

    public static AiOptions Get(IConfiguration configuration)
    {
        var options = new AiOptions();
        configuration.GetSection("AI").Bind(options);

        return options;
    }
}

public record AiModelOptions
{
    public string Default { get; set; } = "deepseek/deepseek-chat";

    public string ChangelogSummary { get; set; } = "deepseek/deepseek-chat";
}
