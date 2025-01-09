using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PallasBot.Domain.Abstract;

public abstract class ScopedTimedBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    protected ScopedTimedBackgroundWorker(TimeSpan interval, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _periodicTimer = new PeriodicTimer(interval);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                await ExecuteInScopeAsync();
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
    }

    protected abstract Task ExecuteInScopeAsync();
}
