using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using PallasBot.Domain.Abstract;
using PallasBot.EntityFrameworkCore.Services;

namespace PallasBot.EntityFrameworkCore;

public static class Extensions
{
    public static void AddEntityFrameworkCore(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");

        builder.Services.AddDbContext<PallasBotDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (builder.Environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

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

        builder.AddDataLayerServices();
    }

    private static void AddDataLayerServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDynamicConfigurationService, DynamicConfigurationService>();
    }
}
