using MassTransit;
using PallasBot.Application.Common.Models.GitHub;
using PallasBot.Application.Common.Models.Messages.GitHub;
using PallasBot.Application.Common.Services;

namespace PallasBot.Application.Command.Consumers;

public class GitHubLoginFetchAccessTokenConsumer : IConsumer<GitHubLoginFetchAccessTokenMqo>
{
    private readonly GitHubApiService _gitHubApiService;

    public GitHubLoginFetchAccessTokenConsumer(GitHubApiService gitHubApiService)
    {
        _gitHubApiService = gitHubApiService;
    }

    public async Task Consume(ConsumeContext<GitHubLoginFetchAccessTokenMqo> context)
    {
        var m = context.Message;

        var slowDown = false;

        while (DateTimeOffset.UtcNow < m.ExpiresAt)
        {
            var interval = Random.Shared.Next(m.Interval * 1000 + 500, m.Interval * 1000 + 1000);
            if (slowDown)
            {
                interval *= 2;
                slowDown = false;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(interval));

            var resp = await _gitHubApiService
                .GetLoginDeviceFlowAccessTokenAsync(m.DeviceCode);

            if (resp.IsFailed)
            {
                if (resp.HasError<GitHubDeviceCodeFlowErrorResponse>() is false)
                {
                    await context.Publish(new GitHubLoginFetchAccessTokenSystemErrorMqo
                    {
                        CorrelationId = m.CorrelationId,
                        Message = $"System error occurred. Please try again later. If the problem persists, please contact the administrators. The attempt id: {m.CorrelationId}",
                    });
                    return;
                }

                var error = (GitHubDeviceCodeFlowErrorResponse)resp.Errors
                    .First(x => x is GitHubDeviceCodeFlowErrorResponse);
                switch (error.Error)
                {
                    case OAuthDeviceCodeFlowError.AuthorizationPending:
                        continue;
                    case OAuthDeviceCodeFlowError.SlowDown:
                        slowDown = true;
                        continue;
                    case OAuthDeviceCodeFlowError.ExpiredToken:
                        await context.Publish(new GitHubLoginFetchAccessTokenUserErrorMqo
                        {
                            CorrelationId = m.CorrelationId,
                            Message = $"The login attempt of code {m.UserCode} expired. You can try again if your accounts are not connected.",
                        });
                        return;
                    case OAuthDeviceCodeFlowError.AccessDenied:
                        await context.Publish(new GitHubLoginFetchAccessTokenUserErrorMqo
                        {
                            CorrelationId = m.CorrelationId,
                            Message = $"You have rejected the login attempt of code {m.UserCode}. You can try again if your accounts are not connected.",
                        });
                        return;
                    case OAuthDeviceCodeFlowError.UnsupportedGrantType:
                    case OAuthDeviceCodeFlowError.IncorrectClientCredentials:
                    case OAuthDeviceCodeFlowError.IncorrectDeviceCode:
                    case OAuthDeviceCodeFlowError.InvalidScope:
                    case OAuthDeviceCodeFlowError.Unknown:
                    default:
                        await context.Publish(new GitHubLoginFetchAccessTokenSystemErrorMqo
                        {
                            CorrelationId = m.CorrelationId,
                            Message = $"System error occurred. Please try again later. If the problem persists, please contact the administrators. The attempt id: {m.CorrelationId}",
                        });
                        return;
                }
            }

            var accessToken = resp.Value;

            await context.Publish(new GitHubLoginFetchAccessTokenSuccessMqo
            {
                CorrelationId = m.CorrelationId,
                AccessToken = accessToken.AccessToken
            });
            return;
        }

        await context.Publish(new GitHubLoginFetchAccessTokenUserErrorMqo
        {
            CorrelationId = m.CorrelationId,
            Message = $"The login attempt of code {m.UserCode} expired. You can try again if your accounts are not connected.",
        });
    }
}
