using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace PallasBot.Domain.Extensions;

public static class MassTransitObserverExtensions
{
    public static void AddSagaStateMachineObserver<TSaga, TStateObserver, TEventObserver>(this IServiceCollection services)
        where TSaga : class, SagaStateMachineInstance
        where TStateObserver : class, IStateObserver<TSaga>
        where TEventObserver : class, IEventObserver<TSaga>
    {
        services.AddStateObserver<TSaga, TStateObserver>();
        services.AddEventObserver<TSaga, TEventObserver>();
    }
}
