using MassTransit;
using PallasBot.Application.Common.Models.Messages;
using PallasBot.Application.Common.Models.Messages.GitHub;
using PallasBot.Domain.Saga;

namespace PallasBot.Application.Common.StateMachine;

public class GitHubLoginStateMachine : MassTransitStateMachine<GitHubLoginSaga>
{
    public State FetchingAccessToken { get; set; } = null!;
    public State UserError { get; set; } = null!;
    public State SystemError { get; set; } = null!;
    public State BindingUser { get; set; } = null!;

    public Event<GitHubLoginStartMqo> Start { get; set; } = null!;
    public Event<GitHubLoginFetchAccessTokenSuccessMqo> FetchAccessTokenSuccess { get; set; } = null!;
    public Event<GitHubLoginFetchAccessTokenUserErrorMqo> FetchAccessTokenUserError { get; set; } = null!;
    public Event<GitHubLoginFetchAccessTokenSystemErrorMqo> FetchAccessTokenSystemError { get; set; } = null!;
    public Event<GitHubLoginResultDmOkMqo> DmOk { get; set; } = null!;
    public Event<GitHubLoginBindingUserOkMqo> BindingUserOk { get; set; } = null!;

    public GitHubLoginStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => Start, x =>
            x.CorrelateById(c => c.Message.CorrelationId));
        Event(() => FetchAccessTokenSuccess, x =>
            x.CorrelateById(c => c.Message.CorrelationId));
        Event(() => FetchAccessTokenUserError, x =>
            x.CorrelateById(c => c.Message.CorrelationId));
        Event(() => FetchAccessTokenSystemError, x =>
            x.CorrelateById(c => c.Message.CorrelationId));
        Event(() => DmOk, x =>
            x.CorrelateById(c => c.Message.CorrelationId));

        Initially(
            When(Start)
                .Then(x =>
                {
                    x.Saga.DiscordUserId = x.Message.DiscordUserId;
                    x.Saga.DeviceCode = x.Message.DeviceCode;
                    x.Saga.UserCode = x.Message.UserCode;
                    x.Saga.ExpiresIn = x.Message.ExpiresIn;
                    x.Saga.Interval = x.Message.Interval;
                    x.Saga.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(x.Message.ExpiresIn);
                })
                .TransitionTo(FetchingAccessToken)
                .Publish(x => new GitHubLoginFetchAccessTokenMqo
                {
                    CorrelationId = x.Saga.CorrelationId,
                    ExpiresAt = x.Saga.ExpiresAt,
                    Interval = x.Saga.Interval,
                    DeviceCode = x.Saga.DeviceCode,
                    UserCode = x.Saga.UserCode
                }));

        During(FetchingAccessToken,
            When(FetchAccessTokenUserError)
                .TransitionTo(UserError)
                .Publish(x => new GitHubLoginResultDmMqo
                {
                    CorrelationId = x.Saga.CorrelationId,
                    DiscordUserId = x.Saga.DiscordUserId,
                    TextMessage = x.Message.Message
                }),
            When(FetchAccessTokenSystemError)
                .TransitionTo(SystemError)
                .Publish(x => new GitHubLoginResultDmMqo
                {
                    CorrelationId = x.Saga.CorrelationId,
                    DiscordUserId = x.Saga.DiscordUserId,
                    TextMessage = x.Message.Message
                }),
            When(FetchAccessTokenSuccess)
                .TransitionTo(BindingUser)
                .Publish(x => new GitHubLoginBindingUserMqo
                {
                    CorrelationId = x.Saga.CorrelationId,
                    DiscordUserId = x.Saga.DiscordUserId,
                    AccessToken = x.Message.AccessToken
                }));

        During(UserError, When(DmOk).Finalize());
        During(SystemError, When(DmOk).Finalize());

        During(BindingUser, When(BindingUserOk).Finalize());
    }
}
