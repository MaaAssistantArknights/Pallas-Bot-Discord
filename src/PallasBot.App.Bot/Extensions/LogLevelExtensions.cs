using Discord;
using Serilog.Events;

namespace PallasBot.App.Bot.Extensions;

public static class LogLevelExtensions
{
    public static LogLevel MapToLogLevel(this LogSeverity severity)
    {
        return severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.None
        };
    }

    public static string ToShort(this LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => "TRC",
            LogEventLevel.Debug => "DBG",
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            LogEventLevel.Fatal => "FAT",
            _ => "U"
        };
    }
}
