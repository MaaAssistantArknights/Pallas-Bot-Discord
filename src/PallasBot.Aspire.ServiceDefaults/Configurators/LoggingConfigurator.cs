using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace PallasBot.Aspire.ServiceDefaults.Configurators;

internal static class LoggingConfigurator
{
    internal static void ConfigureSerilog(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog(cfg =>
        {
            if (builder.Environment.IsProduction())
            {
                cfg.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
                cfg.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
                cfg.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning);
            }

            cfg
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .WriteTo.Async(x => x.Console());

            // This is just a simple approach to configure
            // Configurations from the IConfiguration still can be used if leave it empty
            var writeToFile = builder.Configuration["SERILOG_WRITE_TO_FILE"];
            if (string.IsNullOrEmpty(writeToFile) is false)
            {
                cfg.WriteTo.File(writeToFile, rollingInterval: RollingInterval.Day);
            }

            // string[] ignoreUrls =  ["/health", "/alive", "/metrics", "/scalar"];

            cfg.Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p =>
                p.StartsWith("/health", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/alive", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/metrics", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/scalar", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/openapi", StringComparison.InvariantCultureIgnoreCase) ||
                p.StartsWith("/favicon", StringComparison.InvariantCultureIgnoreCase)
            ));

            cfg.ReadFrom.Configuration(builder.Configuration);
        }, writeToProviders: true);
    }
}
