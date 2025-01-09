using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PallasBot.Domain.Abstract;

namespace PallasBot.Application.Command;

public static class Extensions
{
    public static void AddApplicationCommandServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDiscordApplicationInitializer, CommandInitializer>();
    }
}
