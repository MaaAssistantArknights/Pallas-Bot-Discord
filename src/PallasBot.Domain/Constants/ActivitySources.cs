using System.Diagnostics;

namespace PallasBot.Domain.Constants;

public static class ActivitySources
{
    public static readonly ActivitySource AppActivitySource = new("PallasBot.App");

    public static readonly ActivitySource CommandActivitySource = new("PallasBot.Application.Command");

    public static readonly ActivitySource CommandAutocompletionActivitySource = new("PallasBot.Application.Command.Autocompletion");

    public static IEnumerable<string> AllActivitySources
    {
        get
        {
            yield return AppActivitySource.Name;
            yield return CommandActivitySource.Name;
            yield return CommandAutocompletionActivitySource.Name;
        }
    }
}
