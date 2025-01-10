using Microsoft.Extensions.Configuration;
using PallasBot.Domain.Abstract;

namespace PallasBot.Application.Common.Options;

public record GitHubOptions : IOptionType<GitHubOptions>
{
    public string ClientId { get; set; } = string.Empty;

    public string InstallationId { get; set; } = string.Empty;

    public string PemFile { get; set; } = string.Empty;

    public static GitHubOptions Get(IConfiguration configuration)
    {
        var options = new GitHubOptions();
        configuration.GetSection("GitHub").Bind(options);

        return options;
    }
}
