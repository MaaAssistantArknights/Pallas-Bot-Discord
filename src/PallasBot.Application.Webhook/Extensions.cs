﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PallasBot.Application.Common.Abstract;
using PallasBot.Application.Webhook.Processors;

namespace PallasBot.Application.Webhook;

public static class Extensions
{
    public static void AddApplicationWebhookServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddKeyedScoped<IWebhookProcessor, GitHubWebhookProcessor>("github");
    }
}
