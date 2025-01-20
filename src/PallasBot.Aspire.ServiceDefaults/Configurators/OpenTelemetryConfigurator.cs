using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PallasBot.Aspire.ServiceDefaults.Internal;
using PallasBot.Aspire.ServiceDefaults.Utils;

namespace PallasBot.Aspire.ServiceDefaults.Configurators;

internal static class OpenTelemetryConfigurator
{
    internal static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });

        builder.Services.AddSingleton<InternalResourceDetector>();

        var resourceBuilder = ResourceBuilder.CreateEmpty()
            .AddDetector(sp => sp.GetRequiredService<InternalResourceDetector>());

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder);

                metrics
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                metrics.AddOtlpExporter();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                var httpClientIgnoreUrls = ((string[])
                [
                    TelemetryEnvironment.OtelExporterOtlpEndpoint ?? string.Empty,
                    TelemetryEnvironment.OtelExporterOtlpMetricsEndpoint ?? string.Empty,
                    TelemetryEnvironment.OtelExporterOtlpTracesEndpoint ?? string.Empty,
                    TelemetryEnvironment.OtelExporterOtlpLogsEndpoint ?? string.Empty
                ]).Where(x => string.IsNullOrEmpty(x) is false).Distinct().ToArray();

                tracing.SetResourceBuilder(resourceBuilder);

                tracing
                    .AddAspNetCoreInstrumentation(aspnet =>
                    {
                        string[] ignoreUrls =  ["/health", "/alive", "/metrics", "/scalar", "/openapi", "/favicon"];

                        aspnet.Filter = ctx =>
                        {
                            return ignoreUrls.All(ignoreUrl => !ctx.Request.Path.StartsWithSegments(ignoreUrl));
                        };
                    })
                    .AddHttpClientInstrumentation(http =>
                    {
                        // Unknown error: Remove the redundant delegate type will cause Rider to mark it as an error
                        // ReSharper disable once RedundantDelegateCreation
                        http.FilterHttpRequestMessage = new Func<HttpRequestMessage, bool>(r =>
                        {
                            var requestUri = r.RequestUri?.AbsoluteUri;
                            if (requestUri is null)
                            {
                                return true;
                            }
                            if (httpClientIgnoreUrls.Length == 0)
                            {
                                return true;
                            }

                            // Will return true if any of the urls in httpClientIgnoreUrls starts with the requestUri
                            // Invert the result to ignore the requestUri (return false for ignore)
                            var result = httpClientIgnoreUrls
                                .Any(x => requestUri.StartsWith(x, StringComparison.OrdinalIgnoreCase));

                            return !result;
                        });
                    });

                tracing.AddOtlpExporter();
            })
            .WithLogging(logger =>
            {
                logger.SetResourceBuilder(resourceBuilder);
                logger.AddOtlpExporter();
            });
    }
}
