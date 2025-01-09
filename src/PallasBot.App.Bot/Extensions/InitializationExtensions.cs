using System.Net;
using Discord;
using Discord.Interactions;
using Discord.Net.WebSockets;
using Discord.Rest;
using Discord.WebSocket;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using MassTransit.Observables;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using PallasBot.App.Bot.Discord;
using PallasBot.App.Bot.Services;
using PallasBot.Application.Command;
using PallasBot.Application.Webhook;
using PallasBot.Domain.Constants;
using PallasBot.EntityFrameworkCore;

namespace PallasBot.App.Bot.Extensions;

public static class InitializationExtensions
{
    public static void AddBotServices(this WebApplicationBuilder builder)
    {
        builder.AddDiscordBot();
        builder.AddHttpClients();

        builder.AddEntityFrameworkCore();
        builder.AddApplicationServices();
        builder.AddMassTransitServices();

        builder.Services.ConfigureOpenTelemetryTracerProvider(tracer =>
        {
            tracer.AddSource(ActivitySources.AllActivitySources.ToArray());
        });
    }

    private static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddApplicationCommandServices();
        builder.AddApplicationWebhookServices();
    }

    private static void AddMassTransitServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(c =>
        {
            c.AddConsumers(typeof(PallasBot.Application.Command.Extensions).Assembly);
            c.AddConsumers(typeof(PallasBot.Application.Webhook.Extensions).Assembly);

            c.UsingInMemory((ctx, cfg) =>
            {
                cfg.ConfigureEndpoints(ctx);
            });
        });

        builder.Services.ConfigureOpenTelemetryMeterProvider(meter =>
        {
            meter.AddMeter(InstrumentationOptions.MeterName);
        });
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracer =>
        {
            tracer.AddSource(DiagnosticHeaders.DefaultListenerName);
        });

        builder.Services.AddBusObserver<BusObservable>();
        builder.Services.AddReceiveEndpointObserver<ReceiveEndpointObservable>();
        builder.Services.AddReceiveObserver<ReceiveObservable>();
        builder.Services.AddConsumeObserver<ConsumeObservable>();
        builder.Services.AddSendObserver<SendObservable>();
        builder.Services.AddPublishObserver<PublishObservable>();
    }

    private static void AddDiscordBot(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var networkProxyHost = configuration.GetValue("HTTPS_PROXY", string.Empty);
        var webProxy = string.IsNullOrEmpty(networkProxyHost) ? null : new WebProxy(networkProxyHost);

        builder.Services.AddSingleton<DiscordSocketConfig>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                GatewayIntents = GatewayIntents.None,
                WebSocketProvider = DefaultWebSocketProvider.Create(webProxy),
                RestClientProvider = url => new DiscordHttpRestClient(url, httpClientFactory)
            };
        });

        builder.Services.AddSingleton<DiscordSocketClient>();

        builder.Services.AddSingleton<DiscordRestConfig>(sp => sp.GetRequiredService<DiscordSocketConfig>());
        builder.Services.AddSingleton<DiscordRestClient>();

        builder.Services.AddSingleton<InteractionServiceConfig>(_ => new InteractionServiceConfig
        {
            UseCompiledLambda = true,
            DefaultRunMode = RunMode.Sync
        });
        builder.Services.AddSingleton<InteractionService>();

        builder.Services.AddHostedService<DiscordHostedService>();
    }

    private static void AddHttpClients(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var networkProxyHost = configuration.GetValue("HTTPS_PROXY", string.Empty);
        var webProxy = string.IsNullOrEmpty(networkProxyHost) ? null : new WebProxy(networkProxyHost);
        var networkProxyEnabled = webProxy is not null;

        builder.Services.AddHttpClient("Default")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Proxy = webProxy,
                UseProxy = networkProxyEnabled
            });

        builder.Services.AddHttpClient("DiscordRest", client =>
            {
                client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false,
                Proxy = webProxy,
                UseProxy = networkProxyEnabled
            });
    }
}
