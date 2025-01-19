using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PallasBot.Domain.Constants;

namespace PallasBot.Domain.Abstract;

[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
public abstract class ScopedTimedBackgroundWorker<TService> : IHostedService, IDisposable
    where TService : notnull
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ScopedTimedBackgroundWorkerOptions _options;
    private readonly ILogger _logger;

    private PeriodicTimer? _periodicTimer;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _taskCancellationTokenSource = new();

    private CancellationToken ServiceCancellationToken { get; set; }

    protected ScopedTimedBackgroundWorker(
        ScopedTimedBackgroundWorkerOptions options,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = loggerFactory.CreateLogger("BackgroundWorker");
    }

    protected abstract string Name { get; }
    protected abstract Task ExecuteAsync(TService service, CancellationToken cancellationToken);

    private async Task ExecuteInScopeAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        var activity = ActivitySources.AppActivitySource.StartActivity($"Job {Name}");
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await ExecuteAsync(service, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred in the execution of job {Name}", Name);
        }
        finally
        {
            activity?.Dispose();
            _semaphore.Release();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ServiceCancellationToken = cancellationToken;
        _periodicTimer = new PeriodicTimer(_options.Interval);

        _ = Task.Run(async () =>
        {
            _logger.LogInformation("Starting job {Name} {JobConfig}", Name, _options);

            if (_options.RunOnStart)
            {
                _logger.LogInformation("Running job {Name} on start", Name);
                await ExecuteInScopeAsync(_taskCancellationTokenSource.Token);
            }

            while (_periodicTimer is not null && _taskCancellationTokenSource.Token.IsCancellationRequested is false)
            {
                try
                {
                    await _periodicTimer.WaitForNextTickAsync(_taskCancellationTokenSource.Token);

                    if (_taskCancellationTokenSource.Token.IsCancellationRequested is false)
                    {
                        _logger.LogInformation("Timer elapsed, running job {Name}", Name);
                        await ExecuteInScopeAsync(_taskCancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Job {Name} was cancelled", Name);
                    return;
                }
            }
        }, ServiceCancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _taskCancellationTokenSource.CancelAsync();
        _periodicTimer?.Dispose();
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
