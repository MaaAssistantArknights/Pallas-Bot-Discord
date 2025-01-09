using PallasBot.App.Bot.Extensions;
using PallasBot.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddDefaultServices();
builder.AddBotServices();

var app = builder.Build();

await app.MigrateDatabaseAsync();

app.ConfigureDefault();

app.MapDefaultEndpoints();
app.MapWebhooks();

await app.RunAsync();
