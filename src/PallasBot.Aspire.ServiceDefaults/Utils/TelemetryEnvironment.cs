namespace PallasBot.Aspire.ServiceDefaults.Utils;

public static class TelemetryEnvironment
{
    public static string OtelServiceName => Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "Unknown";

    public static string? OtelExporterOtlpEndpoint => Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
    public static string? OtelExporterOtlpMetricsEndpoint => Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT");
    public static string? OtelExporterOtlpTracesEndpoint => Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT");
    public static string? OtelExporterOtlpLogsEndpoint => Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT");
}
