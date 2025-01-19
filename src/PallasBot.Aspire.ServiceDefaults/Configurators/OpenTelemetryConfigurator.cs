using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using PallasBot.Aspire.ServiceDefaults.Utils;

namespace PallasBot.Aspire.ServiceDefaults.Configurators;

internal static class OpenTelemetryConfigurator
{
    internal static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otelSvcName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "Unknown";
        var probabilityString = builder.Configuration["OTEL_TRACING_DEFAULT_PROBABILITY"];
        var probability = Math.Clamp(
            double.TryParse(probabilityString, out var parsed) ? parsed : 1.0,
            0.0, 1.0);

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }
                else
                {
                    tracing.SetSampler(new TraceIdRatioBasedSampler(probability));
                }

                tracing.AddSource(otelSvcName);

                var httpClientIgnoreUrls = ((string[])
                [
                    TelemetryEnvironment.OtelExporterOtlpEndpoint ?? string.Empty,
                    TelemetryEnvironment.OtelExporterOtlpMetricsEndpoint ?? string.Empty,
                    TelemetryEnvironment.OtelExporterOtlpTracesEndpoint ?? string.Empty,
                    TelemetryEnvironment.OtelExporterOtlpLogsEndpoint ?? string.Empty
                ]).Where(x => string.IsNullOrEmpty(x) is false).Distinct().ToArray();

                tracing
                    .AddAspNetCoreInstrumentation(aspnet =>
                    {
                        string[] ignoreUrls =  ["/health", "/alive", "/metrics", "/scalar"];

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
            });

        builder.AddOpenTelemetryExporters();
    }

    private static void AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var genericOtelEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var meterOtelEndpoint = builder.Configuration["OTEL_EXPORTER_METRICS_ENDPOINT"];
        var tracingOtelEndpoint = builder.Configuration["OTEL_EXPORTER_TRACING_ENDPOINT"];

        if (string.IsNullOrEmpty(genericOtelEndpoint ?? meterOtelEndpoint) is false)
        {
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
            {
                metrics.AddOtlpExporter();
            });
        }

        if (string.IsNullOrEmpty(genericOtelEndpoint ?? tracingOtelEndpoint) is false)
        {
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
            {
                tracing.AddOtlpExporter();
            });
        }
    }
}
