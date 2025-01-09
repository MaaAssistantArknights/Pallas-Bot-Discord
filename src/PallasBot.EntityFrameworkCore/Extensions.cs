using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace PallasBot.EntityFrameworkCore;

public static class Extensions
{
    public static void AddEntityFrameworkCore(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<PallasBotDbContext>();

        builder.Services.ConfigureOpenTelemetryMeterProvider(providerBuilder =>
        {
            providerBuilder.AddNpgsqlInstrumentation();
        });
        builder.Services.ConfigureOpenTelemetryTracerProvider(providerBuilder =>
        {
            providerBuilder.AddNpgsql();
        });

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<PallasBotDbContext>();
    }
}
