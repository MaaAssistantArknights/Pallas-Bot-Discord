using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PallasBot.Application.Common.Models;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.App.Bot.Extensions;

public static class ApplicationExtensions
{
    public static async Task MigrateDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<PallasBotDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PallasBotDbContext>>();

        var migrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

        if (migrations.Count == 0)
        {
            return;
        }

        await dbContext.Database.MigrateAsync();

        foreach (var migration in migrations)
        {
            logger.LogInformation("Migrated database: {Migration}", migration);
        }
    }

    public static void ConfigureDefault(this WebApplication app)
    {
    }

    public static void MapWebhooks(this WebApplication app)
    {
        app.MapPost("/webhook/{processor}", async (
            [FromServices] IPublishEndpoint endpoint,
            [FromRoute] string processor,
            [FromBody] string body) =>
        {
            await endpoint.Publish(new WebhookMessage
            {
                Processor = processor,
                Body = body
            });
            return TypedResults.NoContent();
        });
    }
}
