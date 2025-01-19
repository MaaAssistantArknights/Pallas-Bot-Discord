﻿using System.Globalization;
using Microsoft.Extensions.Hosting;
using PallasBot.Aspire.ServiceDefaults.Utils;
using Serilog;
using Serilog.Filters;
using Serilog.Sinks.OpenTelemetry;

namespace PallasBot.Aspire.ServiceDefaults.Configurators;

internal static class LoggingConfigurator
{
    internal static void ConfigureSerilog(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog(cfg =>
        {
            cfg
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);

            // string[] ignoreUrls =  ["/health", "/alive", "/metrics", "/scalar"];

            cfg.Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p =>
                p.StartsWith("/health", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/alive", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/metrics", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/scalar", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/openapi", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/favicon", StringComparison.InvariantCultureIgnoreCase)
            ));

            var otelEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ??
                               builder.Configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"];
            var otelProtocolString = builder.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] ??
                               builder.Configuration["OTEL_EXPORTER_OTLP_LOGS_PROTOCOL"] ??
                                 "grpc";
            var otelProtocol = otelProtocolString switch
            {
                "grpc" => OtlpProtocol.Grpc,
                "http/protobuf" => OtlpProtocol.HttpProtobuf,
                _ => OtlpProtocol.Grpc
            };

            if (string.IsNullOrEmpty(otelEndpoint))
            {
                return;
            }

            cfg.WriteTo.OpenTelemetry(options =>
            {
                options.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField;
                options.Endpoint = otelEndpoint;
                options.LogsEndpoint = otelEndpoint;
                options.Protocol = otelProtocol;

                var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "Unknown";
                var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"].ParseAsDictionary();
                var resources = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"].ParseAsDictionary();

                // Headers
                foreach (var (k, v) in headers)
                {
                    options.Headers[k] = v;
                }

                // Resource Attributes
                foreach (var (k, v) in resources)
                {
                    options.ResourceAttributes[k] = v;
                }

                // Service Name
                if (options.ResourceAttributes.ContainsKey("service.name") is false)
                {
                    options.ResourceAttributes["service.name"] = serviceName;
                }
            });
        });
    }
}
