using PallasBot.App.Bot.Extensions;
using PallasBot.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddDefaultServices();
builder.AddDefaultWebServices();
builder.AddBotServices();

var app = builder.Build();

await app.MigrateDatabaseAsync();

app.MapDefaultEndpoints(7128);
app.MapWebhooks();

await app.RunAsync();
