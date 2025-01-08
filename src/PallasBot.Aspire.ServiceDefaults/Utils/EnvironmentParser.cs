namespace PallasBot.Aspire.ServiceDefaults.Utils;

internal static class EnvironmentParser
{
    internal static Dictionary<string, string> ParseAsDictionary(this string? source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return [];
        }

        var split = source.Split(',');
        var result = new Dictionary<string, string>();

        foreach (var s in split)
        {
            var index = s.IndexOf('=');

            if (index == -1)
            {
                throw new InvalidOperationException($"Invalid header format: {s}");
            }

            var key = s[..index];
            var value = s[(index + 1)..];

            result.Add(key, value);
        }

        return result;
    }
}
