using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace PallasBot.Aspire.AppHost.Extensions;

public static class DistributedAppExtensions
{
    public static IDistributedApplicationBuilder ConfigureAppHost(this IDistributedApplicationBuilder builder)
    {
        builder.ConfigureSerilog();

        return builder;
    }

    public static IResourceBuilder<IResourceWithConnectionString> AddResourceWithConnectionString(
        this IDistributedApplicationBuilder builder,
        Func<IDistributedApplicationBuilder, IResourceBuilder<IResourceWithConnectionString>> resourceBuilder,
        string connectionStringName)
    {
        var csValue = builder.Configuration.GetConnectionString(connectionStringName);
        return string.IsNullOrEmpty(csValue)
            ? resourceBuilder.Invoke(builder)
            : builder.AddConnectionString(connectionStringName);
    }

    public static IResourceBuilder<IResourceWithConnectionString> AddConnectionStringWithDefault(
        this IDistributedApplicationBuilder builder,
        string connectionStringName,
        string defaultValue)
    {
        var csValue = builder.Configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrEmpty(csValue))
        {
            builder.Configuration.AddInMemoryCollection([
                new KeyValuePair<string, string?>($"ConnectionStrings:{connectionStringName}", defaultValue)
            ]);
        }

        return builder.AddConnectionString(connectionStringName);
    }

    private static IDistributedApplicationBuilder ConfigureSerilog(this IDistributedApplicationBuilder builder)
    {
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();

            var cfg = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            if (builder.Environment.IsProduction())
            {
                cfg.MinimumLevel.Override("Aspire.Hosting.Dcp", LogEventLevel.Warning);
            }

            loggingBuilder.AddSerilog(cfg.CreateLogger());
        });

        return builder;
    }
}
