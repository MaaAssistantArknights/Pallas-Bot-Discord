using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PallasBot.Domain.Abstract;

public abstract class ScopedTimedBackgroundWorker : IHostedService, IDisposable
{
    private readonly ScopedTimedBackgroundWorkerOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private PeriodicTimer? _periodicTimer;

    protected CancellationToken CancellationToken { get; set; }
    protected ILogger Logger { get; }

    protected ScopedTimedBackgroundWorker(
        ScopedTimedBackgroundWorkerOptions options,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;

        Logger = loggerFactory.CreateLogger("BackgroundWorker");
    }

    protected abstract string Name { get; }
    protected abstract Task ExecuteInScopeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        _periodicTimer = new PeriodicTimer(_options.Interval);
        _ = Task.Run(async () =>
        {
            if (_options.RunOnStart)
            {
                await ExecuteAsync(cancellationToken);
            }

            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                await ExecuteAsync(cancellationToken);
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _periodicTimer?.Dispose();
        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        using var scope = _serviceProvider.CreateScope();
        try
        {
            await ExecuteInScopeAsync(scope.ServiceProvider, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error occurred while executing the background worker {WorkerName}", Name);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _periodicTimer?.Dispose();
    }
}

public record ScopedTimedBackgroundWorkerOptions
{
    public required TimeSpan Interval { get; init; }

    public bool RunOnStart { get; set; }
}
