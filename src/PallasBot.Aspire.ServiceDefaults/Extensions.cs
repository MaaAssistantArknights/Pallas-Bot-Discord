using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PallasBot.Aspire.ServiceDefaults.Configurators;
using PallasBot.Aspire.ServiceDefaults.OpenApi;
using PallasBot.Aspire.ServiceDefaults.Utils;
using Scalar.AspNetCore;

namespace PallasBot.Aspire.ServiceDefaults;

public static class Extensions
{
    public static void AddDefaultServices(this IHostApplicationBuilder builder)
    {
        builder.ConfigureConfigurations();
        builder.ConfigureOpenTelemetry();
        builder.ConfigureHealthChecks();
        builder.ConfigureSerilog();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
    }

    public static void AddDefaultWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();

        builder.Services.AddOpenApi(TelemetryEnvironment.OtelServiceName, options =>
        {
            options.AddDocumentTransformer<DefaultApiTransformer>();
        });
        builder.Services.AddCors(cors =>
        {
            cors.AddDefaultPolicy(p =>
            {
                p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
            });
        });
    }

    public static void MapDefaultEndpoints(this WebApplication app, int devPort = 443)
    {
        app.UseCors();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All
        });

        var healthChecks = app.MapGroup("");

        healthChecks
            .CacheOutput("HealthChecks")
            .WithRequestTimeout("HealthChecks");

        healthChecks.MapHealthChecks("/health");
        healthChecks.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = static r => r.Tags.Contains("live")
        });

        if (app.Environment.IsProduction() is false)
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options.AddServer($"https://localhost:{devPort}"));

            app.MapGet("/", () => TypedResults.Redirect($"/scalar/{TelemetryEnvironment.OtelServiceName}"))
                .ExcludeFromDescription();
        }
    }

    private static void ConfigureConfigurations(this IHostApplicationBuilder builder)
    {
        var configurationFile = Path.GetFullPath(
            Environment.GetEnvironmentVariable("PALLAS_BOT_CONFIGURATION_FILE") ?? "appsettings.yaml");
        var configurationFileDirectory = Path.GetDirectoryName(configurationFile)!;
        var configurationFileWithoutExt = Path.GetFileNameWithoutExtension(configurationFile);

        var envSpecificFile = Path.Combine(configurationFileDirectory,
            $"{configurationFileWithoutExt}.{builder.Environment.EnvironmentName.ToLowerInvariant()}.yaml");

        if (File.Exists(configurationFile))
        {
            builder.Configuration.AddYamlFile(configurationFile);
        }

        builder.Configuration.AddYamlFile(envSpecificFile, true);
        builder.Configuration.AddEnvironmentVariables();
    }
}
