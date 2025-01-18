using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PallasBot.Domain.Extensions;

public static class DependencyInjectionExtensions
{
    public static TReturn RunInScope<TService, TReturn>(this IServiceProvider serviceProvider, Func<TService, TReturn> operation)
        where TService : notnull
    {
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return operation.Invoke(service);
    }

    public static void AddScopedTimedBackgroundWorker<TWorker, TService>(this IServiceCollection services)
        where TWorker : class, IHostedService
        where TService : class
    {
        services.AddScoped<TService>();
        services.AddHostedService<TWorker>();
    }
}
