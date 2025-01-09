namespace PallasBot.App.Bot.Discord;

public readonly struct DiscordMultipartFile
{
    public Stream Stream { get; }
    public string Filename { get; }
    public string? ContentType { get; }

    public DiscordMultipartFile(Stream stream, string filename, string? contentType = null)
    {
        Stream = stream;
        Filename = filename;
        ContentType = contentType;
    }
}
