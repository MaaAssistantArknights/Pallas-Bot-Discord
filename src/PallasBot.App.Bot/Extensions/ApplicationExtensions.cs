using System.ComponentModel;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PallasBot.Application.Common.Models.Messages;
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

    public static void MapWebhooks(this WebApplication app)
    {
        app.MapPost("/webhook/{processor}", async (
                HttpContext ctx,
                [FromRoute, Description("Webhook processor to use")] string processor) =>
            {
                var endpoint = ctx.RequestServices.GetRequiredService<IPublishEndpoint>();

                using var bodyReader = new StreamReader(ctx.Request.Body);
                var body = await bodyReader.ReadToEndAsync();

                var headers = ctx.Request.Headers
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value
                            .Where(s => string.IsNullOrEmpty(s) is false)
                            .Cast<string>()
                            .ToArray());

                await endpoint.Publish(new WebhookMessageMqo
                {
                    Processor = processor,
                    Body = body,
                    Headers = headers
                });
                return TypedResults.NoContent();
            })
            .WithName("Webhook receiver")
            .WithDescription("Receive and process webhooks from external services")
            .WithTags("Webhook");
    }
}
